# EcoBridge API

EcoBridge is a food-rescue platform that connects donors, charities, and volunteers. Donors publish surplus food, charities claim donations, volunteers deliver them, and the platform tracks delivery missions, notifications, trust, reviews, and reward points.

This README documents the current backend contract for front-end integration. It describes routes and data shapes that are present in the current codebase. Endpoints that are not currently exposed by a controller are listed in **Known gaps and integration notes** for completion.

## Contents

- [Architecture](#architecture)
- [Running locally](#running-locally)
- [HTTP and authentication](#http-and-authentication)
- [Response envelope](#response-envelope)
- [Roles](#roles)
- [Main workflow](#main-workflow)
- [HTTP endpoint catalog](#http-endpoint-catalog)
- [Request and response models](#request-and-response-models)
- [Enums](#enums)
- [Events and real-time updates](#events-and-real-time-updates)
- [Domain models](#domain-models)
- [Application handlers](#application-handlers)
- [Errors and validation](#errors-and-validation)
- [Known gaps and integration notes](#known-gaps-and-integration-notes)

## Architecture

The solution is split into four runtime projects:

| Project | Responsibility |
| --- | --- |
| `EcoBridgeAPI` | ASP.NET Core host, controllers, middleware, Swagger, JWT, SignalR |
| `Application` | MediatR commands, queries, handlers, DTOs, services, events, background jobs |
| `Domain` | Entities, value rules, enums, domain errors, lifecycle transitions |
| `Infrastructure` | EF Core SQL Server context, migrations, Redis, Cloudinary, geographic data |

The application uses:

- ASP.NET Core controllers with routes under `/api`.
- MediatR for commands, queries, and notifications.
- SQL Server for persistent data.
- Redis for refresh-token validation and selected caches.
- Hangfire for background jobs.
- SignalR for mission and delivery updates.
- Cloudinary for optional image uploads.
- JWT authentication with role-based authorization.

## Running locally

### Docker Compose

The repository includes SQL Server, Redis, and API services:

```bash
docker compose up --build
```

The API is exposed at:

```text
http://localhost:8080
```

The compose file supplies development values for SQL Server, Redis, JWT, and the API port. Replace these values for any shared or production environment.

### Local .NET run

Prerequisites:

- .NET SDK compatible with `net10.0`.
- SQL Server.
- Redis.
- Optional Cloudinary account for image uploads.

Run the API from the repository root:

```bash
dotnet restore EcoBridge_Project.slnx
dotnet run --project EcoBridgeAPI/EcoBridgeAPI.csproj
```

Configure the following settings in `EcoBridgeAPI/appsettings.json`, user secrets, or environment variables:

```text
ConnectionStrings:DefaultConnection
Redis:ConnectionString
Redis:InstancePrefix
JWT:SecretKey
JWT:Issuer
JWT:Audience
JWT:TokenLifetimeMinutes
JWT:RefreshTokenLifetimeDays
Cloudinary:CloudName
Cloudinary:ApiKey
Cloudinary:ApiSecret
```

Swagger/OpenAPI is registered by the API host. Use the Swagger URL emitted by the running ASP.NET Core application.

## HTTP and authentication

### Base URL

All controller routes are prefixed with `/api`:

```text
http://localhost:8080/api
```

For example:

```text
GET /api/Donation?page=1&pageSize=10
```

### Authenticated requests

Protected routes use JWT authentication. The access token identifies the user through the `sub` or `NameIdentifier` claim. Role-protected routes require one of the roles documented below.

The intended browser flow is:

1. Call `POST /api/Auth/register` or `POST /api/Auth/login`.
2. Keep the returned access-token expiry time in client state.
3. Send authenticated requests with the access token according to the deployment's JWT transport policy.
4. Call `POST /api/Auth/refresh` when the access token expires. The refresh controller reads `accessToken` and `refreshToken` from request cookies and rotates the refresh token.
5. Call `POST /api/Auth/revoke` when logging out.

The current `AuthResponseDTO` returns both token strings, but the refresh endpoint is implemented around cookies. The repository's cookie writer currently uses `AccessToken` and `RefreshToken`, while the controller reads lowercase `accessToken` and `refreshToken`. Verify the deployed cookie names before wiring automatic refresh.

For cross-origin browser calls, send credentials when cookies are used:

```js
fetch(`${API_BASE}/api/Auth/refresh`, {
  method: 'POST',
  credentials: 'include'
});
```

### Rate limits

Security-sensitive authentication routes use the `SecurityFixedWindow` policy. General authenticated routes use `GeneralTokenBucket`. A rate-limited response should be treated as temporary and retried with backoff.

## Response envelope

Successful controller responses normally use this shape:

```json
{
  "isSuccess": true,
  "code": "DONATION_RETRIEVED",
  "data": {}
}
```

For operations without data:

```json
{
  "isSuccess": true,
  "code": "LOGOUT_SUCCESS",
  "data": null
}
```

Paged responses put the collection inside `data`:

```json
{
  "isSuccess": true,
  "code": "SUCCESS",
  "data": {
    "data": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 0,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Failure responses use the same envelope where possible:

```json
{
  "isSuccess": false,
  "code": "INVALID_CREDENTIALS",
  "data": null
}
```

Validation failures may return a string in `data`. HTTP status codes are generally `200`, `201`, `400`, `401`, `403`, `404`, or `409`, depending on the failure type.

## Roles

| Role | Value | Main responsibility |
| --- | ---: | --- |
| `Admin` | 1 | Governance, statistics, user status, administrative assignment |
| `Donor` | 2 | Create and manage donations |
| `Charity` | 3 | Claim donations, receive deliveries, confirm delivery with OTP |
| `Volunteer` | 4 | Accept delivery missions and update pickup/delivery status |

Registration accepts `Donor`, `Charity`, or `Volunteer`. Public registration of `Admin` is rejected.

## Main workflow

### Donation lifecycle

The normal status path is:

```text
Available
  -> Accepted
  -> SearchingForVolunteer
  -> VolunteerAssigned
  -> InTransit
  -> Delivered
```

Other terminal or recovery states are `Cancelled` and `Expired`. The domain also defines `Pending`.

Typical sequence:

1. A donor creates a donation with `POST /api/Donation` using `multipart/form-data`. New donations start as `Available`.
2. A charity claims it with `POST /api/Donation/{id}/accept`. The donation becomes `Accepted`.
3. The application starts volunteer search when the appropriate command is invoked. The donation becomes `SearchingForVolunteer`.
4. A volunteer assigns themselves with `POST /api/Donation/{id}/assign`, or an admin assigns a target volunteer. The donation becomes `VolunteerAssigned` and a delivery mission is created.
5. The volunteer confirms pickup through the status endpoint. The donation becomes `InTransit`.
6. Delivery completion is verified using the mission OTP flow. The donation becomes `Delivered`, the volunteer becomes available again, and reward points are credited.
7. A donor, charity, or admin can cancel where authorized. Expiry and volunteer timeout are also handled by background jobs.

### Review and trust workflow

A participant in a delivery mission can submit a review for another participant. The reviewer and reviewee must both belong to the mission, and duplicate reviewer/reviewee/mission combinations are rejected.

A negative review publishes `TrustScoreDeductedEvent`, which:

- Adds a penalty log.
- Decreases the reviewee's trust score by 10.
- Invalidates profile caches.
- Creates a `PenaltyApplied` notification.
- Publishes `TrustScoreDepletedEvent` when the score reaches zero.

A depleted trust score deactivates the account, revokes its refresh tokens, invalidates profile caches, sends a suspension notification, and broadcasts `AccountSuspended` over SignalR.

### Gamification workflow

A successfully completed mission awards the volunteer 50 wallet points and creates a `MissionCompleted` point transaction. The points and trust score are available from the dashboard and gamification endpoints.

## HTTP endpoint catalog

All paths below are relative to `/api`.

### Authentication and users

| Method | Path | Auth | Body / query | Result |
| --- | --- | --- | --- | --- |
| `POST` | `/Auth/register` | Public | `RegisterRequestDTO` JSON | `AuthResponseDTO` |
| `POST` | `/Auth/login` | Public | `LoginRequestDTO` JSON | `AuthResponseDTO` |
| `POST` | `/Auth/refresh` | Cookie-based refresh | No body | Rotated `AuthResponseDTO` |
| `POST` | `/Auth/revoke?revokeAll=false` | Any authenticated user | Query flag | Boolean or code-only success |
| `GET` | `/Auth/me` | Authenticated | None | `UserProfileDTO` |
| `PUT` | `/Auth/me` | Authenticated | `UpdateProfileDTO` multipart form | `UserProfileDTO` |
| `POST` | `/Auth/change-password` | Authenticated | `ChangePasswordDTO` JSON | Boolean |
| `POST` | `/Auth/forgot-password` | Public | `ForgotPasswordRequestDTO` JSON | Boolean |
| `POST` | `/Auth/reset-password` | Public | `ResetPasswordRequestDTO` JSON | Boolean |
| `POST` | `/Auth/verify-email` | Public | `VerifyEmailRequestDTO` JSON | Boolean |
| `GET` | `/Users/me` | Authenticated | None | `UserProfileDTO` |
| `PUT` | `/Users/me` | Authenticated | `UpdateProfileDTO` multipart form | `UserProfileDTO` |
| `GET` | `/Users/{id}/public-profile` | Public | Guid route value | `PublicProfileDTO` |

`/Auth/me` and `/Users/me` currently expose the same profile operations.

### Donations

| Method | Path | Auth | Body / query | Result |
| --- | --- | --- | --- | --- |
| `POST` | `/Donation` | `Donor` | `CreateDonationDTO` multipart form | `DonationResponseDTO`, normally HTTP 201 |
| `GET` | `/Donation` | Any of `Charity`, `Admin`, `Donor`, `Volunteer` | `cityId`, `categoryId`, `status`, `page` or `pageNumber`, `pageSize` | Paged donations |
| `GET` | `/Donation/{id}` | Authenticated | Guid route value | `DonationResponseDTO` |
| `PUT` | `/Donation/{id}` | `Donor` | `UpdateDonationDTO` multipart form | `DonationResponseDTO` |
| `DELETE` | `/Donation/{id}` | `Donor` | Guid route value | Code-only success |
| `POST` | `/Donation/{id}/accept` | `Charity` | No body | Donation or operation result |
| `POST` | `/Donation/{id}/assign` | `Volunteer` or `Admin` | Optional `AssignVolunteerDTO` JSON | Mission/donation result |
| `PATCH` | `/Donation/{id}/status` | `Volunteer` | `UpdateStatusDTO` JSON | Donation or operation result |
| `PATCH` | `/Donation/{id}/cancel` | `Donor`, `Charity`, or `Admin` | `CancelDonationDTO` JSON | Donation or operation result |

Donation list defaults to page 1 and page size 10. Handler-level bounds normalize invalid page sizes to 20 in the query handler, while the controller default is 10; clients should always send an explicit `pageSize`.

### Charity

| Method | Path | Auth | Result |
| --- | --- | --- | --- |
| `GET` | `/Charity/my-accepted-donations` | `Charity` | List of non-pending `DonationResponseDTO` |

### Dashboards

| Method | Path | Auth | Result |
| --- | --- | --- | --- |
| `GET` | `/Dashboard/donor` | `Donor` or `Admin` | `DonorDashboardDTO` |
| `GET` | `/Dashboard/volunteer` | `Volunteer` or `Admin` | `VolunteerDashboardDTO` |
| `GET` | `/Dashboard/charity` | `Charity` or `Admin` | `CharityDashboardDTO` |

### Missions

| Method | Path | Auth | Result |
| --- | --- | --- | --- |
| `GET` | `/Missions/my` | `Volunteer` | List of `MissionDTO` |
| `GET` | `/Missions/{id}` | Authenticated | `MissionDTO` |
| `GET` | `/Missions/{id}/otp` | `Charity` | OTP result used to verify delivery |

### Reviews, notifications, and gamification

| Method | Path | Auth | Body / query | Result |
| --- | --- | --- | --- | --- |
| `POST` | `/Reviews` | Authenticated mission participant | `CreateReviewDTO` JSON | `ReviewResponseDTO` |
| `GET` | `/Notifications` | Authenticated | `page`, `pageSize` | Paged notifications |
| `GET` | `/Notifications/unread-count` | Authenticated | None | Unread count |
| `PATCH` or `PUT` | `/Notifications/{id}/read` | Authenticated | Notification id | Operation result |
| `PATCH` or `PUT` | `/Notifications/read-all` | Authenticated | None | Operation result |
| `GET` | `/Gamification/my-points` | Authenticated | `page`, `pageSize` | `MyPointsResponseDTO` |
| `GET` | `/Gamification/leaderboard` | Public | `role`, `page`, `pagesize` | Paged `LeaderboardUserDTO` |

### Lookups and geographic data

| Method | Path | Auth | Result |
| --- | --- | --- | --- |
| `GET` | `/Lookups/categories` | Public | List of `LookupDTO` |
| `GET` | `/Lookups/countries` | Public | List of `LookupDTO` |
| `GET` | `/Lookups/governorates/{countryId}` | Public | List of `LookupDTO` |
| `GET` | `/Lookups/cities/{governorateId}` | Public | List of `LookupDTO` |
| `GET` | `/Lookups/review-reasons` | Public | List of `ReviewReasonLookupDTO` |
| `GET` | `/Geographic/countries` | Public | Country lookup list |
| `GET` | `/Geographic/countries/{countryId}/governorates` | Public | Governorate lookup list |
| `GET` | `/Geographic/governorates/{governorateId}/cities` | Public | City lookup list |
| `GET` | `/Meta/governorates` | Public | Governorate list |
| `GET` | `/Meta/governorates/{id}/cities` | Public | City list |
| `GET` | `/Meta/review-reasons` | Public | Review reason list |

There are overlapping lookup surfaces (`Lookups`, `Geographic`, and `Meta`) because they are backed by different services. Use one surface consistently in the front end after confirming the desired contract.

### Administration

| Method | Path | Auth | Body / query | Result |
| --- | --- | --- | --- | --- |
| `GET` | `/admin/stats` | `Admin` | None | `AdminStatsDTO` |
| `GET` | `/admin/users` | `Admin` | Optional `role` | List of `UserDTO` |
| `PATCH` | `/admin/users/{id}/toggle-status` | `Admin` | User Guid | Updated status result |
| `GET` | `/admin/donations` | `Admin` | `page`, `pagesize` | Paged admin donation result |

## Request and response models

### Authentication

```json
// LoginRequestDTO
{
  "email": "user@example.com",
  "password": "string"
}
```

```json
// RegisterRequestDTO
{
  "fullName": "Full Name",
  "email": "user@example.com",
  "phone": "+201000000000",
  "password": "Password1",
  "role": "Donor",
  "countryId": 1,
  "governorateId": 1,
  "cityId": 1,
  "addressDetails": "Optional details"
}
```

```json
// AuthResponseDTO
{
  "accessToken": "jwt",
  "refreshToken": "refresh-token",
  "accessTokenExpiresAtUtc": "2026-09-21T12:00:00Z"
}
```

### Profile forms

`UpdateProfileDTO` is `multipart/form-data`:

| Field | Type | Notes |
| --- | --- | --- |
| `fullName` | string | Required, max 150 |
| `phone` | string | Required, max 20 |
| `cityId` | integer | Optional |
| `vehicleType` | string | Optional, max 50 |
| `vehicleDetails` | string | Optional, max 100 |
| `organizationName` | string | Optional, max 150 |
| `registrationNumber` | string | Optional, max 50 |
| `address` | string | Optional, max 255 |
| `addressDetails` | string | Optional, max 500 |
| `profileImage` | file | Optional |

### Donation forms

`CreateDonationDTO` is `multipart/form-data`:

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `categoryId` | integer | No | Lookup category id |
| `title` | string | No | Max 200 |
| `description` | string | No | Max 1000 |
| `foodType` | string | No | Max 100 |
| `quantity` | string | Yes | Max 50; preserves descriptions such as `10 boxes` |
| `weight` | decimal | No | Weight in kilograms |
| `expiryDate` | ISO datetime | Yes | UTC recommended |
| `pickupLocation` | string | Yes | Max 300 in request |
| `cityId` | integer | No | Defaults to donor city |
| `image` | file | No | Requires Cloudinary to be enabled |

`UpdateDonationDTO` is also `multipart/form-data` and contains optional `foodType`, `quantity`, `expiryDate`, `pickupLocation`, `cityId`, and `image` fields.

```json
// DonationResponseDTO
{
  "id": "00000000-0000-0000-0000-000000000000",
  "categoryId": 1,
  "title": "Prepared meals",
  "description": "Optional description",
  "foodType": "Meals",
  "quantity": 10,
  "quantityDes": "10 boxes",
  "weight": 12.5,
  "expiryDate": "2026-09-21T18:00:00Z",
  "pickupLocation": "Pickup address",
  "cityId": 1,
  "imageUrl": "https://...",
  "status": "Available",
  "donorId": "00000000-0000-0000-0000-000000000000",
  "donorName": "Donor name",
  "createdAt": "2026-09-21T10:00:00Z"
}
```

### Mission, review, and notification models

```json
// MissionDTO
{
  "id": "mission-guid",
  "donationRequestId": "donation-guid",
  "volunteerId": "volunteer-guid",
  "volunteerName": "Volunteer name",
  "missionStatus": "VolunteerAssigned",
  "pickupLocation": "Pickup address",
  "dropOffLocation": "Charity address",
  "pickupTime": null,
  "deliveryTime": null,
  "createdAt": "2026-09-21T10:00:00Z",
  "foodType": "Meals",
  "quantity": "10 boxes"
}
```

```json
// CreateReviewDTO
{
  "missionId": "mission-guid",
  "revieweeId": "user-guid",
  "reviewReasonId": 1,
  "rating": 5,
  "comment": "Optional comment"
}
```

`rating` is accepted by the DTO but the current review handler derives positive/negative behavior from the selected review reason.

```json
// NotificationDTO
{
  "id": 1,
  "recipientId": "user-guid",
  "title": "Mission Assigned",
  "body": "A mission was assigned to you.",
  "type": "MissionAssigned",
  "isRead": false,
  "createdAtUtc": "2026-09-21T10:00:00Z",
  "relatedEntityId": "mission-guid"
}
```

### Dashboards and public profiles

`DonorDashboardDTO` contains `totalDonationsCreated`, `totalItemsSaved`, `activePendingRequests`, `totalCompletedDonations`, `totalPoints`, and `trustScore`.

`VolunteerDashboardDTO` contains `completedDeliveries`, `totalPoints`, `activePenaltyWarnings`, `activeDeliveriesCount`, and `trustScore`.

`CharityDashboardDTO` contains `totalFoodReceived`, `pendingRequests`, `activeDeliveriesInTransit`, `approvedRequestsCount`, and `trustScore`.

`PublicProfileDTO` contains `id`, `displayName`, `avatarUrl`, `averageRating`, `totalReviews`, `totalActivities`, `joinDate`, `role`, `trustScore`, `city`, and `organizationName`.

`UserProfileDTO` contains `id`, `fullName`, `email`, `phone`, `role`, `userType`, `totalWalletPoints`, `trustScore`, `cityId`, `vehicleType`, `address`, `addressDetails`, `imagePath`, and `createdAt`.

### Lookup models

```json
// LookupDTO
{
  "id": 1,
  "name": "Cairo",
  "code": "CAI"
}
```

```json
// ReviewReasonLookupDTO
{
  "id": 1,
  "code": "ON_TIME",
  "name": "ON_TIME",
  "isPositive": true,
  "fromRole": "Charity",
  "toRole": "Volunteer"
}
```

## Enums

Values are serialized as strings by the API JSON configuration. Enum model binding also supports string input on controller requests where the enum is used.

### `DonationStatus`

`Available`, `Pending`, `SearchingForVolunteer`, `Accepted`, `InTransit`, `Delivered`, `Cancelled`, `Expired`, `VolunteerAssigned`

### `UserRole`

`Admin`, `Donor`, `Charity`, `Volunteer`

### `UserType`

`Individual`, `Company`

### `NotificationType`

`MissionAvailable`, `MissionAssigned`, `MissionTaken`, `DeliveryCompleted`, `PenaltyApplied`, `DonationStatusChanged`

### `TransactionType`

`MissionCompleted`, `PointsRedeemed`, `Penalty`

A mission reward is a positive transaction. Redemptions and penalties are negative transactions.

## Events and real-time updates

### SignalR connection

The authenticated hub is:

```text
/hubs/missions
```

The client must provide the JWT according to the SignalR deployment configuration. On connection, the hub places the user in `User_{userId}`. Volunteers with a registered city also join normalized city, governorate, and country groups.

Hub methods available to clients:

- `JoinCityGroup(cityName)` and `LeaveCityGroup(cityName)`
- `JoinGovernorateGroup(governorateName)` and `LeaveGovernorateGroup(governorateName)`
- `JoinCountryGroup(countryName)` and `LeaveCountryGroup(countryName)`

Server-to-client events:

| Event | Payload | Meaning |
| --- | --- | --- |
| `MissionAvailable` | mission id, donation id, locations, geographic names, created time | A delivery mission is available |
| `MissionTaken` | `missionId`, `volunteerId` | Another volunteer accepted the mission |
| `MissionInTransit` | `missionId` | Pickup was confirmed |
| `DeliveryCompleted` | `missionId`, optional `pointsAwarded` | Delivery was verified |
| `AccountSuspended` | `accountId`, `reason`, `message` | Trust score reached zero |

### MediatR domain events

| Event | Published when | Main effects |
| --- | --- | --- |
| `MissionCreatedEvent` | A delivery mission is created | Broadcasts availability and creates volunteer notifications |
| `MissionAcceptedEvent` | A volunteer accepts/gets assigned | Broadcasts `MissionTaken` and notifies the volunteer |
| `MissionInTransitEvent` | Pickup is confirmed | Notifies charity and broadcasts `MissionInTransit` |
| `DeliveryCompletedEvent` | OTP delivery verification succeeds | Awards 50 points, restores volunteer availability, notifies participants |
| `TrustScoreDeductedEvent` | Negative review is submitted | Adds penalty, decreases trust score, notifies reviewee |
| `TrustScoreDepletedEvent` | Trust score reaches zero | Deactivates account, revokes tokens, notifies and broadcasts suspension |

## Domain models

The main persisted models and front-end relevance are:

| Model | Important fields / relationships |
| --- | --- |
| `Account` | Identity user with role, type, profile, city, availability, wallet points, trust score, active/deleted state |
| `Donation` | Donor, optional charity and volunteer, category, city, food, quantity, weight, expiry, pickup location, image, status |
| `DeliveryMission` | Donation, volunteer, pickup/drop-off locations, pickup/delivery times, OTP state, cancellation state |
| `Review` | Reviewer, reviewee, mission, review reason, comment, creation time |
| `ReviewReason` | Code, positive flag, source role, target role |
| `Notification` | Recipient, title, body, type, read state, timestamp, related entity |
| `PointTransaction` | User, signed amount, transaction type, optional reference, timestamp |
| `PenaltyLog` | Trust penalty audit information and mission reference |
| `RefreshToken` | Refresh token ownership, expiry, revocation state |
| `Country`, `Governorate`, `City` | Geographic hierarchy used by profiles, filtering, and volunteer targeting |
| `Category` | Donation category lookup |

Important domain rules:

- Trust score starts at 100 and is bounded between 0 and 100.
- Wallet points start at 0.
- A volunteer is made available again after successful delivery.
- Mission OTPs are six digits, expire after 15 minutes, and allow at most five attempts.
- Soft-deleted donations, accounts, and missions are excluded from normal reads.
- A donation cannot be claimed after expiry or claimed twice.
- A review must target another participant in the same mission.

## Application handlers

The application follows a MediatR request/handler pattern. The current handler inventory is:

### Authentication

- `RegisterCommandHandler`
- `LoginCommandHandler`
- `RefreshTokenCommandHandler`
- `RevokeTokenCommandHandler`
- `ChangeUserPasswordCommandHandler`
- `ForgotPasswordCommandHandler`
- `ResetPasswordCommandHandler`
- `VerifyEmailCommandHandler`
- `GetUserProfileQueryHandler`
- `UpdateUserProfileCommandHandler`

### Donations

- `CreateDonationCommandHandler`
- `UpdateDonationCommandHandler`
- `DeleteDonationCommandHandler`
- `ClaimDonationCommandHandler`
- `AssignVolunteerCommandHandler`
- `UpdateDonationStatusCommandHandler`
- `CancelDonationCommandHandler`
- `GetDonationByIdQueryHandler`
- `GetDonationsQueryHandler`
- `GetMyDonationsByStatusQueryHandler`
- `GetAvailableTasksQueryHandler`

### Missions, reviews, and users

- `GetMyMissionsQueryHandler`
- `GetMissionByIdQueryHandler`
- `GetMissionOtpQueryHandler`
- `CreateReviewCommandHandler`
- `GetReviewsByUserIdQueryHandler`
- `GetPublicProfileQueryHandler`

### Dashboards and gamification

- `GetDonorDashboardQueryHandler`
- `GetVolunteerDashboardQueryHandler`
- `GetCharityDashboardQueryHandler`
- `GetMyPointsQueryHandler`
- `GetLeaderboardQueryHandler`

### Lookups and administration

- `GetLookupCategoriesQueryHandler`
- `GetLookupCountriesQueryHandler`
- `GetLookupGovernoratesQueryHandler`
- `GetLookupCitiesQueryHandler`
- `GetLookupReviewReasonsQueryHandler`
- `GetAdminStatsQueryHandler`
- `GetAdminUsersQueryHandler`
- `ToggleUserStatusCommandHandler`
- `GetAdminDonationsQueryHandler`

## Errors and validation

The front end should branch on `code`, not on localized message text. Common codes include:

| Area | Codes |
| --- | --- |
| Authentication | `INVALID_CREDENTIALS`, `EMAIL_ALREADY_EXISTS`, `ACCOUNT_DEACTIVATED`, `INVALID_TOKEN`, `TOKEN_EXPIRED`, `REFRESH_TOKEN_REVOKED` |
| Authorization | `UNAUTHORIZED`, `FORBIDDEN` |
| Donations | `DONATION_NOT_FOUND`, `DONATION_ALREADY_ACCEPTED`, `DONATION_NOT_ACCEPTED`, `DONATION_INVALID_STATUS`, `DONATION_ALREADY_HAS_ACTIVE_MISSION`, `DONATION_EXPIRED` |
| Participants | `DONOR_NOT_FOUND`, `CHARITY_NOT_FOUND`, `CHARITY_NOT_VERIFIED`, `VOLUNTEER_NOT_FOUND`, `VOLUNTEER_INACTIVE` |
| Missions | `MISSION_NOT_FOUND`, `MISSION_ALREADY_DELIVERED`, `MISSION_ALREADY_CANCELLED`, `MISSION_NOT_IN_TRANSIT`, `OTP_EXPIRED`, `INVALID_OTP`, `OTP_ATTEMPTS_EXCEEDED` |
| Reviews | `REVIEW_INVALID_MISSION`, `REVIEW_ALREADY_EXISTS` |
| Files | `FILE_REQUIRED`, `FILE_TOO_LARGE`, `INVALID_FILE_TYPE`, `IMAGE_UPLOAD_FAILED`, `UPLOAD_FAILED` |
| Profile and validation | `VALIDATION_ERROR`, `INVALID_ROLE`, `INVALID_CITY_ID`, `FULL_NAME_REQUIRED`, `EMAIL_REQUIRED`, `PHONE_REQUIRED`, `PASSWORD_TOO_WEAK` |
| Gamification | `POINTS_RETRIEVED`, `LEADERBOARD_RETRIEVED`, `TRUST_SCORE_LIMIT_EXCEEDED` |

Use `401` for missing or invalid authentication, `403` for a valid user without permission, `404` for missing resources, `409` for state conflicts, and `400` for malformed or invalid input.

## Known gaps and integration notes

These items should be confirmed or added to this README as the API surface is completed:

1. The current controllers expose mission OTP retrieval but do not expose a controller endpoint that accepts the volunteer's OTP and completes delivery. Add the final request and response contract when that endpoint is available.
2. `GetAvailableTasksQuery` exists in the application layer but is not currently exposed by `MissionsController` or `DonationController`. Confirm whether the donation list or a dedicated mission route is the intended front-end entry point.
3. The domain contains explicit volunteer-search transitions, but the public controller catalog has no dedicated `start-search` or `unclaim` route. Document the final route if these actions are intentionally public.
4. `CompleteMissionRequestDTO` and `AcceptMissionResponseDTO` exist but are not currently used by a visible controller action. Confirm their final endpoint ownership and response shape.
5. `CreateVolunteerDTO` and `VolunteerDTO` exist, but no volunteer-management controller routes are currently listed. Add admin or profile routes if volunteer creation and availability are part of the front-end flow.
6. The leaderboard controller defaults `page` to 20 and `pagesize` to 20, while the handler treats these as ordinary page values. Clients should send explicit `page=1&pageSize=20` or the final corrected parameter names.
7. `GET /Donation` accepts both `page` and `pageNumber`; `pageNumber` wins when both are present. Use one final parameter name in the front end.
8. The auth token response and cookie refresh behavior should be standardized. In particular, confirm cookie names, `HttpOnly`, `Secure`, `SameSite`, and whether browser clients should send `Authorization: Bearer` headers, cookies, or both.
9. `Meta`, `Geographic`, and `Lookups` overlap. Keep one canonical lookup section in the front-end client after the preferred route family is agreed.
10. Add concrete JSON examples for any newly added endpoints, especially admin donation results, OTP completion, available missions, review history, and volunteer management.

## Suggested front-end integration order

1. Load countries, governorates, cities, categories, and review reasons.
2. Register or log in and establish the token/cookie strategy.
3. Load `/Auth/me` and the role-specific dashboard.
4. Implement donor creation and donation list/detail views.
5. Implement charity claim and accepted-donation views.
6. Implement volunteer mission list, assignment, pickup status, OTP completion, and SignalR updates.
7. Add notifications, reviews, public profiles, points, and leaderboard.
8. Add admin-only screens after the admin response shapes are finalized.
