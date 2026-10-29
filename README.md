# 🌍 EcoBridge API — Food Surplus Redistribution Platform

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=dotnet)](https://learn.microsoft.com/ef/core/)
[![Redis](https://img.shields.io/badge/Redis-Cache%20%26%20Tokens-DC382D?logo=redis)](https://redis.io/)
[![Hangfire](https://img.shields.io/badge/Hangfire-Background%20Jobs-orange)](https://www.hangfire.io/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--Time-blue)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)

**EcoBridge** is an enterprise-grade RESTful API built on **ASP.NET Core 9** and **Clean Architecture** principles. It connects **Donors** (restaurants, supermarkets, caterers, individuals) having surplus fresh or packaged food with registered **Charities**, facilitated by a verified network of **Volunteers** for delivery, secured by **OTP verification**, **Gamification (Points & Badges)**, and a dynamic **Trust & Credit Score Engine**.

---

## 📑 Table of Contents

1. [Architectural Overview & Tech Stack](#-architectural-overview--tech-stack)
2. [Project Structure](#-project-structure)
3. [Domain Models & Entities](#-domain-models--entities)
4. [Domain Enums](#-domain-enums)
5. [Security & Authentication Architecture](#-security--authentication-architecture)
6. [Rate Limiting Policies](#-rate-limiting-policies)
7. [SignalR Real-Time Gateway (`/hubs/missions`)](#-signalr-real-time-gateway-hubsmissions)
8. [Background Jobs & Outbox Processor](#-background-jobs--outbox-processor)
9. [Standard Response Envelope & Error Handling](#-standard-response-envelope--error-handling)
10. [Complete API Endpoints Reference](#-complete-api-endpoints-reference)
    - [1. Authentication & Profiles (`/api/Auth`)](#1-authentication--profiles-apiauth)
    - [2. Donation Lifecycle (`/api/Donation`)](#2-donation-lifecycle-apidonation)
    - [3. Delivery Missions (`/api/Missions`)](#3-delivery-missions-apimissions)
    - [4. Charity Operations (`/api/Charity`)](#4-charity-operations-apicharity)
    - [5. Role Dashboards (`/api/Dashboard`)](#5-role-dashboards-apidashboard)
    - [6. Gamification & Leaderboard (`/api/Gamification`)](#6-gamification--leaderboard-apigamification)
    - [7. Reviews & Ratings (`/api/Reviews`)](#7-reviews--ratings-apireviews)
    - [8. Notifications (`/api/Notifications`)](#8-notifications-apinotifications)
    - [9. User Profiles (`/api/Users`)](#9-user-profiles-apiusers)
    - [10. Admin Management (`/api/admin`)](#10-admin-management-apiadmin)
    - [11. Lookups & Geographics (`/api/Lookups`)](#11-lookups--geographics-apilookups)
11. [Data Transfer Objects (DTO) Catalog](#-data-transfer-objects-dto-catalog)
12. [Database Schema & Migrations](#-database-schema--migrations)
13. [Getting Started & Local Setup](#-getting-started--local-setup)

---

## 🏛 Architectural Overview & Tech Stack

The solution follows strict **Clean Architecture / Onion Architecture** and **Domain-Driven Design (DDD)** patterns:

```
                  ┌───────────────────────────────┐
                  │         EcoBridgeAPI          │
                  │  Controllers, Filters, Hubs,  │
                  │     Middlewares, Program.cs   │
                  └───────────────┬───────────────┘
                                  │
                  ┌───────────────▼───────────────┐
                  │          Application          │
                  │  CQRS (MediatR), Behaviors,   │
                  │  DTOs, Validators, Services   │
                  └───────┬───────────────┬───────┘
                          │               │
        ┌─────────────────▼─────┐   ┌─────▼─────────────────────────┐
        │        Domain         │   │        Infrastructure         │
        │ Aggregates, Entities, │   │ EF Core DbContext, Redis,     │
        │ Enums, Domain Events  │   │ Cloudinary, Brevo, Hangfire   │
        └───────────────────────┘   └───────────────────────────────┘
```

- **Framework**: .NET 9.0 (C# 13)
- **Database**: Microsoft SQL Server 2022 via Entity Framework Core 9
- **Caching & Refresh Tokens**: Redis (StackExchange.Redis) + In-Memory L1 Caching
- **Mediation & CQRS**: MediatR with pipeline behaviors (`ValidationBehavior`, `CachingPipelineBehavior`)
- **Validation**: FluentValidation with automatic model state interception
- **File & Media Storage**: Cloudinary .NET SDK
- **Email Delivery**: Brevo (Sendinblue) transactional email API
- **Background Jobs**: Hangfire (SQL Server storage) + Hosted Services
- **Real-Time Communication**: ASP.NET Core SignalR with geo-partitioned groups
- **API Documentation**: OpenAPI / Swagger UI
- **Containerization**: Docker & Docker Compose

---

## 📂 Project Structure

```
EcoBridge_Project/
├── EcoBridgeAPI/                # Presentation Layer (API Host)
│   ├── Controllers/             # RESTful API Controllers (12 Controllers)
│   ├── Filters/                 # CSRF Validation Filter, Action Filters
│   ├── Middleware/              # GlobalExceptionMiddleware, Error Handlers
│   ├── Program.cs               # Service registrations & Middleware pipeline
│   └── appsettings.json         # Configuration keys
├── Application/                 # Application Core Layer
│   ├── BackgroundJobs/          # Recurring job implementations
│   ├── Contracts/               # Abstractions (IEcoBridgeDbContext, etc.)
│   ├── DTO/                     # Request and Response transfer models
│   ├── Extensions/              # Pipeline behaviors (Caching, Validation)
│   ├── Features/                # CQRS Commands, Queries, and Event Handlers
│   ├── Hubs/                    # SignalR MissionHub definition
│   ├── Results/                 # ApiResult, ApiResult<T>, PagedResult<T>
│   └── Services/                # Application-level service interfaces
├── Domain/                      # Domain Core Layer (No Infrastructure dependencies)
│   ├── Constants/               # Domain-wide constants
│   ├── Contracts/               # IAggregateRoot, ISoftDeletable, IDomainEvent
│   ├── Enums/                   # Persisted domain enums
│   ├── Errors/                  # AppMessageCodes catalog
│   ├── Exceptions/              # DomainException hierarchy
│   └── Models/                  # Aggregates, Entities & Partial classes
├── Infrastructure/              # External Concerns & Persistence
│   ├── BackgroundJobs/          # Hangfire job runners & hosted services
│   ├── Binders/                 # Custom enum string model binders
│   ├── Data/                    # EcoBridgeDbContext, Fluent API Configurations
│   ├── Migrations/              # EF Core Code-First migrations
│   └── Services/                # Redis, Cloudinary, Brevo, Photo, JWT implementations
└── tests/                       # Unit & Integration test suites
```

---

## 🧱 Domain Models & Entities

All domain models enforce business invariants through aggregate roots, factory methods, and domain event emissions:

| Class | Type | Key Properties | Responsibilities / Domain Invariants |
| :--- | :--- | :--- | :--- |
| **`Account`** | Aggregate Root | `Id` (Guid), `FullName`, `Email`, `Phone`, `Role` (UserRole), `UserType`, `TrustScore`, `TotalWalletPoints`, `CityId`, `Address`, `VehicleType`, `OrganizationName`, `RegistrationNumber`, `IsActive`, `IsDeleted` | User aggregate (extending Identity). Controls trust credit increments, penalty deductions, profile updates, account activation/suspension, and soft deletes. |
| **`Donation`** | Aggregate Root | `Id` (Guid), `DonorId`, `CategoryId`, `Title`, `Description`, `FoodType`, `QuantityDes`, `Weight`, `ExpiryDate`, `PickupLocation`, `CityId`, `ImageUrl`, `Status` (DonationStatus), `CharityId`, `ClaimedAtUtc`, `DeliveryMissionId`, `RowVersion` | Core donation aggregate. Enforces status lifecycle (`Available` ➔ `SearchingForVolunteer` ➔ `VolunteerAssigned` ➔ `Completed` / `Cancelled` / `Expired`). Uses optimistic concurrency via `RowVersion`. |
| **`DeliveryMission`** | Entity (Guid) | `Id`, `DonationRequestId`, `VolunteerId`, `MissionStatus`, `PickupLocation`, `DropOffLocation`, `PickupTime`, `DeliveryTime`, `OtpHash`, `OtpExpiresAtUtc`, `OtpAttempts`, `MaxOtpAttempts` | Tracks transportation of a claimed donation from donor to charity. Issues cryptographic OTP to charity, verifies volunteer handover, records timestamps and delivery failures. |
| **`Review`** | Entity (int) | `Id`, `MissionId`, `ReviewerId`, `RevieweeId`, `ReviewReasonId`, `Rating` (1–5), `Comment`, `CreatedAt`, `IsDeleted` | Multi-directional rating system (Donor ↔ Volunteer ↔ Charity). Modifies trust scores based on rating value and positive/negative reason codes. |
| **`ReviewReason`** | Entity (int) | `Id`, `Code`, `NameAr`, `NameEn`, `IsPositive`, `FromRole`, `ToRole` | Standardized catalog of review tags (e.g., punctuality, food packaging, professional conduct). |
| **`PointTransaction`**| Entity (int) | `Id`, `AccountId`, `Amount`, `TransactionType`, `ReferenceId`, `CreatedAt` | Immutable ledger of gamification points awarded for completed missions or deducted for violations. |
| **`PenaltyLog`** | Entity (int) | `Id`, `AccountId`, `Reason`, `PenaltyPoints`, `CreatedAt`, `RelatedEntityId` | Historical record of penalties applied against volunteers or charities (e.g., timeout abandonment, mission failure). |
| **`TrustCreditLog`** | Entity (int) | `Id`, `AccountId`, `Points`, `Reason`, `CreatedAt` | Audit log tracking fluctuations in a user's `TrustScore` (starts at 100). |
| **`Notification`** | Entity (int) | `Id`, `RecipientId`, `Title`, `Body`, `Type` (NotificationType), `IsRead`, `CreatedAtUtc`, `RelatedEntityId` | Persistent in-app notifications dispatched simultaneously via SignalR. |
| **`OutboxMessage`** | Entity (Guid) | `Id`, `Type`, `Content` (JSON), `OccurredOnUtc`, `ProcessedOnUtc`, `Error` | Implements transactional Outbox pattern to guarantee reliable event dispatch even under network partitions. |
| **`Category`** | Entity (int) | `Id`, `NameAr`, `NameEn`, `Description`, `IsActive` | Classification of food donations (e.g., Cooked Meals, Bakery, Canned Goods, Fruits & Vegetables). |
| **`Country`** | Entity (int) | `Id`, `Code`, `NameAr`, `NameEn` | Geographic top-level hierarchy (Egypt seeded by default). |
| **`Governorate`** | Entity (int) | `Id`, `CountryId`, `Code`, `NameAr`, `NameEn` | 27 Egyptian governorates. |
| **`City`** | Entity (int) | `Id`, `GovernorateId`, `Code`, `NameAr`, `NameEn` | Cities partitioned under governorates used for localized mission broadcasting. |
| **`RefreshToken`** | Entity (int) | `Id`, `AccountId`, `TokenHash`, `ExpiresAtUtc`, `CreatedAtUtc`, `RevokedAtUtc`, `ReplacedByTokenHash` | Token rotation audit log stored in Redis with secondary persistence in SQL Server. |

---

## 🏷 Domain Enums

### `DonationStatus` (`byte`)
Represents the state machine of a food donation:
- `Available` (`0`): Created by donor; waiting for a charity to claim it.
- `SearchingForVolunteer` (`2`): Claimed by charity; awaiting an available volunteer.
- `VolunteerAssigned` (`3`): A volunteer accepted the delivery mission.
- `Cancelled` (`4`): Cancelled by donor, charity, or system administrator.
- `Expired` (`5`): Expiry date reached before delivery completion (automated via Hangfire).
- `Completed` (`6`): Successfully delivered to and confirmed by the charity via OTP.

### `MissionStatus` (`int`)
Represents the active transport state of a delivery mission:
- `Assigned` (`1`): Volunteer assigned; delivery pending pickup.
- `InTransit` (`2`): Volunteer confirmed food pickup from the donor.
- `Delivered` (`3`): Handed over to charity with verified OTP.
- `Cancelled` (`4`): Cancelled before completion.
- `Failed` (`5`): Delivery failed or timed out.

### `UserRole` (`byte`)
- `Admin` (`1`): Full platform governance, user activation, audit, and analytics.
- `Donor` (`2`): Creates and manages surplus food donations.
- `Charity` (`3`): Claims available donations, receives deliveries, generates verification OTPs.
- `Volunteer` (`4`): Accepts delivery missions, picks up donations, inputs OTP upon delivery.

### `UserType` (`byte`)
- `Individual` (`1`): Independent donors or volunteers.
- `Company` (`2`): Registered restaurants, hotels, grocery chains, or NGOs.

### `NotificationType` (`byte`)
- `MissionAvailable` (`0`), `MissionAssigned` (`1`), `MissionTaken` (`2`), `DeliveryCompleted` (`3`), `PenaltyApplied` (`4`), `DonationStatusChanged` (`5`), `TrustScoreIncreased` (`6`).

### `TransactionType` (`byte`)
- `MissionCompleted` (`0`), `PointsRedeemed` (`1`), `Penalty` (`2`), `UnassignCompensation` (`3`).

### `ReviewRole` (`int`)
- `Reviewer` (`0`): The entity providing the review.
- `Reviewee` (`1`): The entity being evaluated.

### `OtpCheckResult` (`int`)
- `Valid` (`0`), `NoOtpIssued` (`1`), `Expired` (`2`), `MaxAttemptsExceeded` (`3`), `Invalid` (`4`), `NotInTransit` (`5`).

---

## 🔐 Security & Authentication Architecture

EcoBridge enforces modern authentication and enterprise defensive standards:

### 1. Dual-Token Architecture (JWT + HttpOnly Cookie)
- **Access Token (JWT)**: Passed via `Authorization: Bearer <token>`. Carries `sub` (User ID), `role`, `email`, and `userType`. Short-lived (15 minutes).
- **Refresh Token**: Stored in a strict `HttpOnly`, `Secure`, `SameSite=Strict` cookie (`refreshToken`). Used at `/api/Auth/refresh` to issue new token pairs without credential re-entry.
- **Revocation**: Blacklisted in Redis instantly upon logout or revocation (`/api/Auth/revoke`).

### 2. Double-Submit Cookie CSRF Defense
State-modifying endpoints utilize the `[ValidateCsrfToken]` action filter:
- When users authenticate, a `csrfToken` cookie is issued alongside a matching CSRF token.
- The client must read the cookie and pass its value in the request header:
  ```http
  X-CSRF-TOKEN: <token_value>
  ```
- If the header and cookie do not match, the request is rejected with `403 Forbidden`.

### 3. Role-Based Access Control (RBAC)
Controllers and actions are decorated with fine-grained ASP.NET Core `[Authorize(Roles = "...")]` attributes, guaranteeing donors, charities, volunteers, and admins only access appropriate endpoints.

---

## ⏱ Rate Limiting Policies

Configured via .NET 9 Built-in Rate Limiting middleware to prevent abuse and brute-force attacks:

| Policy Name | Limiter Algorithm | Limit & Window | Partition Key | Intended Usage |
| :--- | :--- | :--- | :--- | :--- |
| **`SecurityFixedWindow`** | Fixed Window | 5 requests / 1 minute | Client IP or User ID | Authentication, OTP verification, password reset |
| **`GeneralTokenBucket`** | Token Bucket | Capacity: 20 tokens<br>Refill: 2 tokens/sec | Client IP or User ID | General queries, writes, profile updates |
| **`ConcurrencyAuth`** | Concurrency Limiter | Max 10 concurrent requests (Queue: 20) | Global Group | Password hashing & intensive crypto operations |
| **`ConcurrencyUpload`** | Concurrency Limiter | Max 5 concurrent uploads (Queue: 10) | Global Group | Cloudinary image upload operations |

---

## 📡 SignalR Real-Time Gateway (`/hubs/missions`)

Real-time coordination between donors, charities, and geographically proximate volunteers is powered by SignalR:

### Connection & Group Membership
Upon connecting to `/hubs/missions`, authenticated users are automatically placed into:
1. `User_{UserId}` — Direct targeted notifications.
2. `Group_City_{normalizedCity}` — Volunteers automatically join their registered city.
3. `Group_Governorate_{normalizedGov}` — Broader regional alerts.
4. `Group_Country_{normalizedCountry}` — Country-wide announcements.

### Client-Callable Methods
Clients can dynamically adjust their geo-subscriptions:
- `JoinCityGroup(string cityName)` / `LeaveCityGroup(string cityName)`
- `JoinGovernorateGroup(string governorateName)` / `LeaveGovernorateGroup(string governorateName)`
- `JoinCountryGroup(string countryName)` / `LeaveCountryGroup(string countryName)`

### Server-to-Client Broadcast Events
| Event Name | Payload | Broadcast Target | Description |
| :--- | :--- | :--- | :--- |
| **`MissionAvailable`** | `MissionDTO` / Mission metadata | City Group / All Volunteers | Triggered when a charity claims a donation; notifies nearby volunteers. |
| **`MissionTaken`** | `{ missionId, volunteerId }` | All Clients / City Group | Broadcast when a volunteer accepts a mission; removes it from available task lists. |
| **`MissionInTransit`** | `{ missionId }` | Donor & Charity User Groups | Broadcast when a volunteer picks up the food from the donor. |
| **`DeliveryCompleted`** | `{ missionId, donationId }` | Donor, Charity & Volunteer | Triggered when charity OTP verification succeeds. |
| **`DonationUnclaimed`** | `{ donationId }` | All Clients / City Group | Triggered when a charity unclaims a donation, returning it to `Available`. |
| **`DonationUnassigned`**| `{ donationId, missionId }` | City Group Volunteers | Triggered when a volunteer unassigns, returning mission to `SearchingForVolunteer`. |
| **`AccountSuspended`** | `{ userId, reason }` | `User_{UserId}` | Dispatched when a user's trust score drops below critical threshold. |
| **`ReviewDeleted`** | `{ reviewId, missionId }` | Involved Parties | Notifies involved users of review removal. |
| **`ReceiveNotification`**| `NotificationDTO` | `User_{UserId}` | Real-time push for all in-app notifications. |

---

## ⚙️ Background Jobs & Outbox Processor

EcoBridge employs **Hangfire** and .NET `IHostedService` to run reliable asynchronous workloads:

| Job Name | Schedule | Implementation | Purpose |
| :--- | :--- | :--- | :--- |
| **`donation-expiry-monitor`** | Every hour (`0 * * * *`) | `DonationExpiryJob` | Scans unclaimed donations past `ExpiryDate`, transitions them to `Expired`, and alerts donors. |
| **`volunteer-timeout-monitor`**| Every hour (`0 * * * *`) | `VolunteerTimeoutJob` | Detects assigned missions with no pickup within timeout limits, unassigns the volunteer, applies penalty points, and re-broadcasts the mission. |
| **`transient-data-cleanup`** | Daily (`0 0 * * *`) | `TransientDataCleanupJob`| Purges expired refresh tokens, old notifications, and completed outbox records. |
| **`outbox-processor`** | Every minute (`*/1 * * * *`) | `OutboxProcessorJob` + Hosted Service | Pulls unprocessed domain events from `OutboxMessages` table and dispatches them via MediatR to guarantee at-least-once delivery. |

---

## 📦 Standard Response Envelope & Error Handling

All endpoints return a uniform response envelope (`ApiResult<T>`):

### Success Response Format
```json
{
  "isSuccess": true,
  "code": "SUCCESS",
  "data": { ... }
}
```

### Paginated Response Format (`PagedResult<T>`)
```json
{
  "isSuccess": true,
  "code": "SUCCESS",
  "data": {
    "data": [ ... ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 42,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Failure Response Format
```json
{
  "isSuccess": false,
  "code": "DONATION_NOT_FOUND",
  "data": null
}
```

### Global HTTP Status Mapping
- `200 OK`: Operation succeeded.
- `201 Created`: Resource successfully created (includes `Location` header).
- `400 Bad Request`: Validation failure (`DomainException.ValidationException`).
- `401 Unauthorized`: Missing or invalid JWT bearer token.
- `403 Forbidden`: Role authorization failed or CSRF token mismatch.
- `404 Not Found`: Entity not found (`DomainException.NotFoundException`).
- `409 Conflict`: Business rule violation or concurrency conflict (`DbUpdateConcurrencyException`).
- `429 Too Many Requests`: Rate limit threshold exceeded.
- `500 Internal Server Error`: Unhandled server exception.

---

## 🚀 Complete API Endpoints Reference

### 1. Authentication & Profiles (`/api/Auth`)

| Method | Endpoint | Allowed Roles | Rate Limit | CSRF | Description |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **POST** | `/api/Auth/register` | Public | SecurityFixedWindow | No | Register a new user (`Donor`, `Charity`, or `Volunteer`). |
| **POST** | `/api/Auth/login` | Public | SecurityFixedWindow | No | Authenticate with email/password; returns JWT and sets `refreshToken` cookie. |
| **POST** | `/api/Auth/refresh` | Public | SecurityFixedWindow | **Yes** | Exchanges expired access token and valid refresh cookie for a new token pair. |
| **POST** | `/api/Auth/revoke` | Authenticated | GeneralTokenBucket | **Yes** | Revokes current or all active refresh tokens for the user (`?revokeAll=true`). |
| **GET** | `/api/Auth/profile` | Authenticated | GeneralTokenBucket | No | Retrieves full profile details of the currently authenticated user. |
| **PUT** | `/api/Auth/volunteer/profile`| `Volunteer` | GeneralTokenBucket | **Yes** | Updates volunteer vehicle details, address, and profile picture (`multipart/form-data`). |
| **PUT** | `/api/Auth/charity/profile` | `Charity` | GeneralTokenBucket | **Yes** | Updates charity organization name, registration number, address, and picture. |
| **PUT** | `/api/Auth/profile` | `Donor, Admin` | GeneralTokenBucket | **Yes** | Updates basic profile information and avatar (`multipart/form-data`). |
| **POST** | `/api/Auth/change-password` | Authenticated | GeneralTokenBucket | **Yes** | Changes password verifying current password against stored hash. |
| **POST** | `/api/Auth/forgot-password` | Public | SecurityFixedWindow | **Yes** | Sends password reset email with token via Brevo. |
| **POST** | `/api/Auth/reset-password` | Public | SecurityFixedWindow | **Yes** | Resets password using valid reset token and email. |
| **POST** | `/api/Auth/verify-email` | Public | SecurityFixedWindow | **Yes** | Validates email address verification token. |

---

### 2. Donation Lifecycle (`/api/Donation`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/Donation` | `Donor` | GeneralTokenBucket | Creates a new food donation with optional picture (`multipart/form-data`). |
| **GET** | `/api/Donation` | `Charity, Admin, Donor, Volunteer` | GeneralTokenBucket | Queries donations with optional filters (`cityId`, `categoryId`, `status`, pagination). |
| **GET** | `/api/Donation/available` | `Charity, Volunteer, Admin` | GeneralTokenBucket | Retrieves paginated open donations in `Available` status. |
| **GET** | `/api/Donation/{id}` | Authenticated | GeneralTokenBucket | Gets details of a specific donation by ID. |
| **GET** | `/api/Donation/{id}/finished-mission` | `Donor` | GeneralTokenBucket | Retrieves complete mission and delivery receipt for a completed donation. |
| **PUT** | `/api/Donation/{id}` | `Donor` | GeneralTokenBucket | Updates food type, quantity, expiry date, or image (only if status is `Available`). |
| **DELETE** | `/api/Donation/{id}` | `Donor` | GeneralTokenBucket | Soft deletes a donation (only if not yet claimed or completed). |
| **POST** | `/api/Donation/{id}/accept` | `Charity` | GeneralTokenBucket | Charity claims an available donation; transitions status to `SearchingForVolunteer`. |
| **POST** | `/api/Donation/{id}/unclaim` | `Charity` | GeneralTokenBucket | Charity releases a claimed donation back to `Available`. |
| **POST** | `/api/Donation/{id}/assign` | `Volunteer, Admin` | GeneralTokenBucket | Volunteer assigns themselves to the mission, or Admin assigns a specific volunteer. |
| **POST** | `/api/Donation/{id}/unassign` | `Volunteer` | GeneralTokenBucket | Volunteer drops out of assigned mission; reopens mission for other volunteers. |
| **PATCH** | `/api/Donation/{id}/cancel` | `Donor, Charity, Admin` | GeneralTokenBucket | Cancels donation with mandatory reason; notifies involved parties. |

---

### 3. Delivery Missions (`/api/Missions`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/Missions/{id}/pickup` | `Volunteer` | GeneralTokenBucket | Confirms volunteer picked up food from donor; status becomes `InTransit`. |
| **POST** | `/api/Missions/{id}/complete`| `Volunteer` | GeneralTokenBucket | Verifies OTP provided by Charity. Upon match, marks mission `Delivered` & awards points. |
| **GET** | `/api/Missions/my` | `Volunteer` | GeneralTokenBucket | Lists paginated missions assigned to or completed by the authenticated volunteer. |
| **GET** | `/api/Missions/{id}` | Authenticated | GeneralTokenBucket | Retrieves detailed mission progress, locations, and timestamps. |
| **GET** | `/api/Missions/{id}/otp` | `Charity` | GeneralTokenBucket | Generates or retrieves secure 6-digit delivery verification OTP for the charity. |

---

### 4. Charity Operations (`/api/Charity`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Charity/my-accepted-donations` | `Charity` | GeneralTokenBucket | Retrieves paginated list of all donations accepted/claimed by the charity. |

---

### 5. Role Dashboards (`/api/Dashboard`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Dashboard/donor` | `Donor, Admin` | GeneralTokenBucket | Aggregated KPI stats: total created, active pending, saved items, points, trust score. |
| **GET** | `/api/Dashboard/volunteer`| `Volunteer, Admin`| GeneralTokenBucket | Aggregated KPI stats: completed deliveries, active deliveries, warnings, trust score. |
| **GET** | `/api/Dashboard/charity` | `Charity, Admin` | GeneralTokenBucket | Aggregated KPI stats: total food received, in-transit deliveries, failed deliveries. |

---

### 6. Gamification & Leaderboard (`/api/Gamification`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Gamification/my-points` | Authenticated | GeneralTokenBucket | Returns total wallet points, trust score, and paginated ledger of point transactions. |
| **GET** | `/api/Gamification/leaderboard` | Public | GeneralTokenBucket | Returns ranked leaderboard of top volunteers or donors filtered by role. |

---

### 7. Reviews & Ratings (`/api/Reviews`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Reviews/{id}` | Authenticated | GeneralTokenBucket | Gets specific review details by ID. |
| **GET** | `/api/Reviews/{id}/reasons` | Authenticated | GeneralTokenBucket | Gets predefined reason details associated with a review. |
| **GET** | `/api/Reviews/mission/{missionId}` | Authenticated | GeneralTokenBucket | Lists all reviews written for a completed delivery mission. |
| **GET** | `/api/Reviews/user/{userId}`| Authenticated | GeneralTokenBucket | Lists reviews received by or given by a user (`?role=Reviewee` or `Reviewer`). |
| **POST** | `/api/Reviews` | Authenticated | GeneralTokenBucket | Submits a star rating (1–5), structured reason tag, and optional comment. |
| **PATCH** | `/api/Reviews/{id}` | Authenticated | GeneralTokenBucket | Modifies comment or rating of an existing review written by the caller. |
| **DELETE** | `/api/Reviews/{id}` | Authenticated | GeneralTokenBucket | Soft deletes a review written by the caller. |

---

### 8. Notifications (`/api/Notifications`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Notifications` | Authenticated | GeneralTokenBucket | Retrieves paginated in-app notifications for the caller. |
| **GET** | `/api/Notifications/unread-count`| Authenticated | GeneralTokenBucket | Returns total count of unread notifications. |
| **PATCH** | `/api/Notifications/{id}/read`| Authenticated | GeneralTokenBucket | Marks a specific notification as read. |
| **PATCH** | `/api/Notifications/read-all` | Authenticated | GeneralTokenBucket | Marks all notifications as read for current user. |

---

### 9. User Profiles (`/api/Users`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Users/{id}/public-profile` | Public | GeneralTokenBucket | Returns public reputation profile: average rating, total activities, badges, city. |

---

### 10. Admin Management (`/api/admin`)

| Method | Endpoint | Allowed Roles | Rate Limit | CSRF | Description |
| :--- | :--- | :--- | :--- | :---: | :--- |
| **GET** | `/api/admin/stats` | `Admin` | GeneralTokenBucket | No | System-wide statistics (donations, volunteers, charities, active counts). |
| **GET** | `/api/admin/users` | `Admin` | GeneralTokenBucket | No | Lists all users with optional role filtering (`?role=Charity`). |
| **PATCH**| `/api/admin/users/{id}/toggle-status` | `Admin` | GeneralTokenBucket | **Yes**| Activates or deactivates a user account (e.g., verifying an NGO or banning a bad actor). |
| **GET** | `/api/admin/donations` | `Admin` | GeneralTokenBucket | No | Comprehensive paginated list of all system donations regardless of status. |

---

### 11. Lookups & Geographics (`/api/Lookups`)

| Method | Endpoint | Allowed Roles | Rate Limit | Description |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/api/Lookups/categories` | Public | GeneralTokenBucket | Returns active food donation categories. |
| **GET** | `/api/Lookups/countries` | Public | GeneralTokenBucket | Returns countries with nested governorates and cities. |
| **GET** | `/api/Lookups/governorates/{countryId}` | Public | GeneralTokenBucket | Returns all governorates for a country (e.g., Egypt). |
| **GET** | `/api/Lookups/cities/{governorateId}` | Public | GeneralTokenBucket | Returns all cities in a specific governorate. |
| **GET** | `/api/Lookups/review-reasons` | Public | GeneralTokenBucket | Returns all review reason tags grouped by role. |

---

## 📋 Data Transfer Objects (DTO) Catalog

### Authentication & Users

#### `RegisterRequestDTO`
```csharp
public class RegisterRequestDTO
{
    public string FullName { get; set; }           // Required, max 150 chars
    public string Email { get; set; }              // Required, valid email format
    public string Phone { get; set; }              // Required, international format
    public string Password { get; set; }           // Required, min 6 characters
    public string Role { get; set; }               // "Donor", "Charity", or "Volunteer"
    public string Type { get; set; }               // "Individual" or "Company"
    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }
    public int? CityId { get; set; }
    public string? AddressDetails { get; set; }
    public string? VehicleDetails { get; set; }    // Volunteer specific
    public string? VehicleType { get; set; }       // e.g. Motorcycle, Car, Van
    public string? OrganizationName { get; set; }  // Charity specific
    public string? RegistrationNumber { get; set; }// Charity NGO registration
}
```

#### `LoginRequestDTO`
```csharp
public class LoginRequestDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

#### `AuthResponseDTO`
```csharp
public class AuthResponseDTO
{
    public string AccessToken { get; set; }
    public DateTime AccessTokenExpiresAtUtc { get; set; }
}
```

#### `RefreshTokenRequestDTO`
```csharp
public class RefreshTokenRequestDTO
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
```

#### `ChangePasswordDTO`
```csharp
public class ChangePasswordDTO
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }        // Min length 6
}
```

#### `ForgotPasswordRequestDTO` & `ResetPasswordRequestDTO`
```csharp
public class ForgotPasswordRequestDTO
{
    public string Email { get; set; }
}

public class ResetPasswordRequestDTO
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}

public class VerifyEmailRequestDTO
{
    public string Email { get; set; }
    public string Token { get; set; }
}
```

#### User Profiles: `UserDTO`, `VolunteerProfileDTO`, `CharityProfileDTO`
```csharp
public class UserDTO : MiniUserDto
{
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalWalletPoints { get; set; }
    public int TrustScore { get; set; } = 100;
    public int? CityId { get; set; }
    public string? Address { get; set; }
    public string? AddressDetails { get; set; }
    public string? ImagePath { get; set; }
}

public class VolunteerProfileDTO : UserDTO
{
    public string? VehicleType { get; set; }
    public string? VehicleDetails { get; set; }
}

public class CharityProfileDTO : UserDTO
{
    public string? OrganizationName { get; set; }
    public string? RegistrationNumber { get; set; }
}
```

#### `PublicProfileDTO`
```csharp
public class PublicProfileDTO
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public float AverageRating { get; set; }
    public float AverageImpression { get; set; }
    public int TotalReviews { get; set; }
    public int TotalActivities { get; set; }
    public DateTime JoinDate { get; set; }
    public string Role { get; set; }
    public int TrustScore { get; set; }
    public string? City { get; set; }
    public string? OrganizationName { get; set; }
}
```

---

### Donations & Workflow

#### `CreateDonationDTO` (`multipart/form-data`)
```csharp
public class CreateDonationDTO
{
    public int CategoryId { get; set; }
    public string Title { get; set; }              // Max length 200
    public string Description { get; set; }        // Max length 1000
    public string FoodType { get; set; }           // Max length 100
    public string Quantity { get; set; }           // Max length 50 (e.g. "25 Meals")
    public decimal? Weight { get; set; }           // Weight in Kg
    public DateTime ExpiryDate { get; set; }       // Must be in the future
    public string PickupLocation { get; set; }     // Address / Coordinates
    public int CityId { get; set; }
    public IFormFile? Image { get; set; }          // Optional photo upload
}
```

#### `UpdateDonationDTO` (`multipart/form-data`)
```csharp
public class UpdateDonationDTO
{
    public string FoodType { get; set; }
    public string Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string PickupLocation { get; set; }
    public int CityId { get; set; }
    public IFormFile? Image { get; set; }
}
```

#### `DonationResponseDTO`
```csharp
public class DonationResponseDTO
{
    public Guid Id { get; set; }
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string FoodType { get; set; }
    public string QuantityDes { get; set; }
    public decimal? Weight { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string PickupLocation { get; set; }
    public int? CityId { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; }
    public Guid DonorId { get; set; }
    public string DonorName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### `CancelDonationDTO`
```csharp
public class CancelDonationDTO
{
    public string Reason { get; set; }             // Required, max 500 chars
}
```

#### `AssignVolunteerDTO`
```csharp
public class AssignVolunteerDTO
{
    public Guid VolunteerId { get; set; }          // Required for Admin assignment
}
```

---

### Missions & Deliveries

#### `MissionDTO`
```csharp
public class MissionDTO
{
    public Guid Id { get; set; }
    public Guid DonationRequestId { get; set; }
    public Guid? VolunteerId { get; set; }
    public string? VolunteerName { get; set; }
    public string MissionStatus { get; set; }
    public string PickupLocation { get; set; }
    public string DropOffLocation { get; set; }
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FoodType { get; set; }
    public string? Quantity { get; set; }
}
```

#### `CompleteMissionRequestDTO`
```csharp
public class CompleteMissionRequestDTO
{
    public string OtpCode { get; set; }            // 6-digit verification code from charity
}
```

#### `DonationMissionDetailDTO`
```csharp
public class DonationMissionDetailDTO
{
    public Guid MissionId { get; set; }
    public string MissionStatus { get; set; }
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime MissionCreatedAt { get; set; }
    public string PickupLocation { get; set; }
    public string DropOffLocation { get; set; }

    public Guid VolunteerId { get; set; }
    public string VolunteerName { get; set; }
    public string? VolunteerPhone { get; set; }
    public string? VolunteerImageUrl { get; set; }

    public Guid DonationId { get; set; }
    public string DonationTitle { get; set; }
    public string? DonationDescription { get; set; }
    public string? FoodType { get; set; }
    public string QuantityDes { get; set; }
    public decimal? Weight { get; set; }
    public string? DonationImageUrl { get; set; }

    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public string? CharityAddress { get; set; }
}
```

---

### Reviews, Gamification & Dashboards

#### `CreateReviewDTO` & `UpdateReviewDTO`
```csharp
public class CreateReviewDTO
{
    public Guid MissionId { get; set; }
    public Guid RevieweeId { get; set; }
    public int ReviewReasonId { get; set; }
    public int Rating { get; set; }                // 1 to 5
    public string? Comment { get; set; }
}

public class UpdateReviewDTO
{
    public string? Comment { get; set; }
    public int? Rating { get; set; }               // 1 to 5
}
```

#### `LeaderboardUserDTO` & `MyPointsResponseDTO`
```csharp
public class LeaderboardUserDTO
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; }
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
    public string? City { get; set; }
}

public class MyPointsResponseDTO
{
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
    public PagedResult<PointTransactionDTO> Transactions { get; set; }
}
```

#### Role Dashboard DTOs
```csharp
public class DonorDashboardDTO
{
    public int TotalDonationsCreated { get; set; }
    public int TotalItemsSaved { get; set; }
    public int ActivePendingRequests { get; set; }
    public int TotalCompletedDonations { get; set; }
    public int TotalPoints { get; set; }
    public int TrustScore { get; set; }
}

public class VolunteerDashboardDTO
{
    public int CompletedDeliveries { get; set; }
    public int TotalPoints { get; set; }
    public int ActivePenaltyWarnings { get; set; }
    public int ActiveDeliveriesCount { get; set; }
    public int TrustScore { get; set; }
}

public class CharityDashboardDTO
{
    public int TotalFoodReceived { get; set; }
    public int ActiveDeliveriesInTransit { get; set; }
    public int CanceldDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public double TrustScore { get; set; }
}
```

#### `AdminStatsDTO`
```csharp
public class AdminStatsDTO
{
    public int TotalDonations { get; set; }
    public int TotalVolunteers { get; set; }
    public int CompletedDeliveries { get; set; }
    public int ActiveDonors { get; set; }
    public int AvailableDonations { get; set; }
    public int AcceptedDonations { get; set; }
    public int PickedUpDonations { get; set; }
    public int DeliveredDonations { get; set; }
    public int CancelledDonations { get; set; }
    public int CancelledMissions { get; set; }
    public int TotalCharities { get; set; }
}
```

---

## 🗄 Database Schema & Migrations

The database is built on **Microsoft SQL Server** via Entity Framework Core:

- **Soft Delete Filters**: Automatically applied to `Account`, `Donation`, and `Review`. Deleted rows are retained with `IsDeleted = 1` and excluded from all regular LINQ queries.
- **Concurrency Control**: `Donation.RowVersion` uses `[Timestamp]` byte array to guarantee atomic donation claim operations and prevent race conditions when two charities accept the same donation simultaneously.
- **Outbox Table**: `OutboxMessages` persists all domain events inside the same transaction as entity changes.
- **Pre-seeded Geographic Data**: Egypt is seeded with all 27 Governorates and associated major cities on first boot in development mode.

To apply migrations manually:
```bash
dotnet ef database update --project Infrastructure --startup-project EcoBridgeAPI
```

---

## 🚀 Getting Started & Local Setup

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- SQL Server (or Docker container)
- Redis (or Docker container)

### 1. Clone & Configure Environment
Clone the repository:
```bash
git clone https://github.com/ahmedragab13579/EcoBridge_Project_API.git
cd EcoBridge_Project
```

Review or modify `EcoBridgeAPI/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=EcoBridgeDb;User Id=sa;Password=Your_Strong_Password123!;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "Redis": "localhost:6379,abortConnect=false"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretLongKeyWithAtLeast32BytesLength!",
    "Issuer": "Eco-Bridge-API",
    "Audience": "Eco-Bridge-Clients",
    "DurationInMinutes": 15,
    "RefreshTokenDurationInDays": 30
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "BrevoSettings": {
    "ApiKey": "your-brevo-api-key",
    "FromEmail": "noreply@ecobridge.com",
    "FromName": "EcoBridge Notifications"
  }
}
```

### 2. Run with Docker Compose (Recommended)
Launch SQL Server 2022, Redis, and the EcoBridge API container with one command:
```bash
docker compose up -d --build
```
The services will be available at:
- **API & Swagger**: `http://localhost:8080/swagger`
- **Hangfire Dashboard**: `http://localhost:8080/hangfire`
- **SignalR Hub**: `ws://localhost:8080/hubs/missions`

### 3. Run Locally without Docker
1. Start local SQL Server and Redis instances.
2. Apply database migrations:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project EcoBridgeAPI
   ```
3. Run the API:
   ```bash
   dotnet run --project EcoBridgeAPI
   ```
4. Access Swagger UI at `https://localhost:7123/swagger` (or the HTTP port configured in your launch profiles).

---

## 🧪 Testing

Run all unit and integration test suites:
```bash
dotnet test
```

---

## 📜 License
This project is licensed under the MIT License. Developed for sustainable community impact and food waste reduction.
