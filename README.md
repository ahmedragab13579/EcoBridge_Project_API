# 🌱 EcoBridge API

EcoBridge is an enterprise-grade, real-time RESTful Web API engineered to connect surplus food donors, charitable organizations, and delivery volunteers to minimize food waste and support vulnerable communities. Built using **.NET 10 / ASP.NET Core**, Entity Framework Core, SQL Server, Redis, SignalR, and JWT Authentication, EcoBridge delivers real-time donation dispatching, automated lifecycle management, trust scoring, and role-based logistics workflows.

---

## 🏗️ Architecture & Technology Stack

The solution follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles:

```
EcoBridge_Project/
├── 🌐 EcoBridgeAPI     -> ASP.NET Core 10 Web API, Controllers, Middleware, SignalR Hubs
├── ⚙️ Application      -> CQRS (MediatR), FluentValidation, DTOs, Event Handlers, Background Jobs
├── 🗄️ Infrastructure   -> EF Core, ASP.NET Identity, SQL Server, Redis Caching, Cloudinary Media
└── 🏛️ Domain           -> Domain Entities, Enums, Domain Events, Aggregate Roots, Result Pattern
```

### 🛠️ Core Tech Stack
- **Framework**: .NET 10 / C# 13
- **Architecture**: Clean Architecture + CQRS (MediatR) + DDD
- **Database & ORM**: MS SQL Server & Entity Framework Core (Code-First)
- **Identity & Security**: ASP.NET Core Identity (`IdentityDbContext<Account, ApplicationRole, Guid>`) + JWT Bearer Tokens (HMAC-SHA256)
- **Caching & Refresh Tokens**: Redis Cache & Distributed Lock Support
- **Real-Time Communication**: ASP.NET Core SignalR (`/hubs/missions`)
- **Background Jobs**: Automated donation expiry, volunteer timeout, and charity verification background workers
- **Media Storage**: Cloudinary integration for image uploads
- **Documentation**: OpenAPI & Swagger UI

---

## 🔑 Hybrid Identifier Strategy (Guid vs. int)

To balance **security against enumeration attacks** for public/business-critical resources with **storage and indexing efficiency** for high-volume logs and lookup data, the system implements a hybrid identifier architecture:

| Category | Primary Key | Entities | Rationale |
|---|---|---|---|
| **Critical Business & Identity Entities** | `Guid` (UUIDv4) | `Account`, `ApplicationRole`, `Donor`, `Charity`, `Volunteer`, `Admin`, `Donation`, `DonationRequest`, `DeliveryMission` | Prevents ID enumeration attacks, decouples ID generation from database round-trips, protects public endpoints. |
| **Non-Critical & Lookup Data** | `int` (Auto-Increment) | `Category`, `Country`, `Governorate`, `City`, `ReviewReason` | Compact indexes, minimal overhead for static taxonomy and lookup master tables. |
| **High-Volume Logs & Sub-Records** | `int` (Auto-Increment) | `Review`, `Notification`, `PenaltyLog`, `PointTransaction`, `RefreshToken` | High write throughput, storage compaction; foreign keys point to `Guid` accounts/missions. |

---

## 🚀 Key Modules & Features

The platform is designed around 4 distinct user roles: **Donor**, **Charity**, **Volunteer**, and **Admin**.

### 1. 🔐 Authentication & Security
- **ASP.NET Identity with Guid PKs**: Custom Identity configuration integrated with clean architecture.
- **JWT & Redis Refresh Tokens**: Dual-tier sliding expiration with Redis caching and token revocation.
- **Role-Based Authorization (RBAC)**: Fine-grained permissions for Donors, Charities, Volunteers, and Admins.
- **Rate Limiting & Protection**: Built-in rate limiting policies protecting public endpoints from abuse.

### 2. 🍱 Donation Lifecycle & Expiry
- **Surplus Posting**: Donors post food listings with categories, quantities, pickup windows, and expiration dates.
- **Media Upload**: Cloudinary image upload with thumbnail generation and secure CDN delivery.
- **Optimistic Concurrency**: `RowVersion` concurrency checks preventing double-claiming by charities.
- **Automated Expiry**: Background workers automatically expire stale donation listings.

### 3. 🤝 Donation Requests & Charity Operations
- **Requesting & Claiming**: Charities browse available donations (`Pending`) and submit `DonationRequest`s.
- **Donor Approval Workflow**: Donors can accept or reject specific charity requests.
- **Timeout Management**: Automatic reallocation or cancellation if requests are unhandled within threshold windows.

### 4. 🚚 Real-Time Logistics, SignalR & Hierarchical Address Matching
- **Mission Lifecycle**: Dynamic mission progression: `SearchingForVolunteer` ➔ `Accepted` ➔ `InTransit` ➔ `Delivered` / `Cancelled`.
- **Geographic Address Hierarchy**: Comprehensive multi-tier location structure (`Country` ➔ `Governorate` ➔ `City` ➔ `AddressDetails`).
- **Multi-Tier SignalR Group Broadcasting**: Volunteers auto-join groups across their address levels on connection to `/hubs/missions`:
  - `Group_City_{city_name}` (Local city channel)
  - `Group_Governorate_{governorate_name}` (Regional governorate channel)
  - `Group_Country_{country_name}` (National country channel)
- **Smart Address-Based Mission Matching**: Algorithm matches delivery missions to volunteers based on exact `Donation.CityId`, with an intelligent tiered fallback to `GovernorateId`.
- **Timeout Enforcement**: Automatic mission revocation and reassignment if a volunteer fails to pick up on schedule.

### 5. ⭐ Reviews, Trust Scores & Penalties
- **Two-Way Feedback**: Reviews and ratings between Donors, Charities, and Volunteers.
- **Points & Gamification**: `PointTransaction` tracking positive community contributions.
- **Penalty System**: `PenaltyLog` and automated trust score deductions on cancelled or abandoned missions.

### 6. 📊 Admin Dashboard & Governance
- **Platform Analytics**: Global metrics on food saved, active volunteers, completed missions, and pending charities.
- **Charity Verification**: Admin workflow to approve or reject charity registrations.
- **User Moderation**: Active status toggling and account suspension.

---

## 💾 Database Schema

```
Accounts (Guid) ──── (1:1) ────> Donors (Guid) ─────── (1:N) ───────> Donations (Guid)
   │ ─────────────── (1:1) ────> Charities (Guid) ──── (1:N) ───────┘     ▲
   │ ─────────────── (1:1) ────> Volunteers (Guid) ── (1:N) ─────────────┼─── (1:1) ─── DeliveryMissions (Guid)
   │ ─────────────── (1:1) ────> Admins (Guid)                             │
   ├──────────────── (1:N) ────> RefreshTokens (int)                      │
   ├──────────────── (1:N) ────> Notifications (int)                      │
   ├──────────────── (1:N) ────> PenaltyLogs (int)                        │
   ├──────────────── (1:N) ────> PointTransactions (int)                  │
   └──────────────── (1:N) ────> Reviews (int) ───────────────────────────┘
```

---

## 🚦 API Endpoints Reference

All endpoints are routed under `/api/`. Protected endpoints require a Bearer token: `Authorization: Bearer <JWT_TOKEN>`.

### 🔑 Authentication & Profile (`/api/auth`, `/api/users`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/register` | Register a new user (`Donor`, `Charity`, `Volunteer`) | Public |
| **POST** | `/api/auth/login` | Authenticate and obtain JWT + Refresh Token | Public |
| **POST** | `/api/auth/refresh` | Renew expired JWT with a valid Refresh Token | Public |
| **POST** | `/api/auth/revoke` | Revoke a refresh token on logout | Authenticated |
| **GET** | `/api/auth/me` | Retrieve profile and role metadata for authenticated user | All Roles |
| **PUT** | `/api/auth/me` | Update user profile details (multipart form) | All Roles |
| **POST** | `/api/auth/change-password` | Update account password | All Roles |
| **POST** | `/api/auth/forgot-password` | Request password reset token via email | Public |
| **POST** | `/api/auth/reset-password` | Reset password using email verification token | Public |
| **POST** | `/api/auth/verify-email` | Confirm email address with token | Public |
| **GET** | `/api/users/me` | Retrieve current authenticated user profile | All Roles |
| **PUT** | `/api/users/me` | Update current user profile | All Roles |
| **GET** | `/api/users/{id:guid}/public-profile` | Get public profile data (points, trust score, reviews) | Public |

### 🍱 Donations (`/api/donation`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/donation` | Create a donation offer (supports multipart image) | Donor |
| **GET** | `/api/donation` | List donations with filters (`cityId`, `categoryId`, `status`, pagination) | All Roles |
| **GET** | `/api/donation/my-donations` | List all donations posted by current Donor | Donor |
| **GET** | `/api/donation/open` | List available open donations (`Pending`) | Charity, Admin |
| **GET** | `/api/donation/{id:guid}` | Get full details of a specific donation | All Roles |
| **PUT** | `/api/donation/{id:guid}` | Update donation details (if still pending) | Donor |
| **DELETE** | `/api/donation/{id:guid}` | Soft-delete a donation listing | Donor, Admin |
| **POST** | `/api/donation/{id:guid}/request` | Submit request to claim a donation | Charity |
| **POST** | `/api/donation/{id:guid}/accept` | Accept a donation offer | Charity |
| **POST** | `/api/donation/{id:guid}/assign` | Assign target volunteer or self to donation mission | Volunteer, Admin |
| **PATCH** | `/api/donation/{id:guid}/status` | Update donation status | Volunteer |
| **PATCH** | `/api/donation/{id:guid}/cancel` | Cancel donation with cancellation reason | Donor, Charity, Admin |

### 🤝 Donation Requests (`/api/donationrequests`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/donationrequests/{donationId:guid}/request` | Charity submits a request to claim a donation | Charity |
| **POST** | `/api/donationrequests/{id:guid}/approve` | Donor accepts a charity's donation request | Donor |

### 🚚 Missions & Logistics (`/api/missions`, `/api/volunteers`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/missions/available` | Browse missions awaiting volunteer pickup | Volunteer, Admin |
| **GET** | `/api/missions/my` | View current volunteer's assigned & ongoing missions | Volunteer |
| **GET** | `/api/missions/{id:guid}` | Get mission tracking details & status | All Roles |
| **POST** | `/api/missions/{id:guid}/accept` | Accept / Claim an available delivery mission | Volunteer |
| **POST** | `/api/missions/{id:guid}/confirm-pickup` | Donor confirms food handover (`InTransit`) | Donor |
| **POST** | `/api/missions/{id:guid}/complete` | Complete mission with OTP validation (`Delivered`) | Volunteer |
| **GET** | `/api/missions/{id:guid}/otp` | Retrieve delivery completion OTP code | Charity |
| **GET** | `/api/volunteers` | List all volunteers | Public / All Roles |
| **GET** | `/api/volunteers/{id:guid}` | Get volunteer details | Public / All Roles |
| **POST** | `/api/volunteers` | Create volunteer profile | Admin |
| **PUT** | `/api/volunteers/{id:guid}` | Update volunteer profile | Admin |
| **DELETE** | `/api/volunteers/{id:guid}` | Remove volunteer profile | Admin |
| **GET** | `/api/volunteers/available-tasks` | Get available delivery tasks | Volunteer, Admin |
| **GET** | `/api/volunteers/my-tasks` | Get assigned tasks for current volunteer | Volunteer |
| **PATCH** | `/api/volunteers/me/availability` | Toggle volunteer active/inactive status | Volunteer |

### ⭐ Reviews & Notifications (`/api/reviews`, `/api/notifications`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/reviews` | Submit rating & review for completed mission | All Roles |
| **GET** | `/api/notifications` | Get user notifications list with pagination | All Roles |
| **GET** | `/api/notifications/unread-count` | Get unread notifications count | All Roles |
| **PATCH** | `/api/notifications/{id:int}/read` | Mark a specific notification as read | All Roles |
| **PATCH** | `/api/notifications/read-all` | Mark all notifications as read | All Roles |

### 📊 Dashboards & Analytics (`/api/dashboard`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/dashboard/donor` | Key metrics for donor (items saved, active requests, trust score) | Donor, Admin |
| **GET** | `/api/dashboard/volunteer` | Key metrics for volunteer (deliveries, points, penalties) | Volunteer, Admin |
| **GET** | `/api/dashboard/charity` | Key metrics for charity (food received, pending requests) | Charity, Admin |

### 🏆 Gamification & Leaderboard (`/api/gamification`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/gamification/my-points` | Point transaction history, wallet total, trust score | Authenticated |
| **GET** | `/api/gamification/leaderboard` | Public leaderboard rankings by role | Public |

### 🗺️ Lookups & Metadata (`/api/lookups`, `/api/meta`, `/api/geographic`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/lookups/categories` | Food categories taxonomy | Public |
| **GET** | `/api/lookups/countries` | Supported countries | Public |
| **GET** | `/api/lookups/governorates/{countryId:int}` | Governorates for a country | Public |
| **GET** | `/api/lookups/cities/{governorateId:int}` | Cities within a governorate | Public |
| **GET** | `/api/lookups/review-reasons` | Predefined review/feedback reason tags | Public |
| **GET** | `/api/meta/governorates` | Cached governorates list | Public |
| **GET** | `/api/meta/governorates/{id:int}/cities` | Cached cities list for governorate | Public |
| **GET** | `/api/meta/review-reasons` | Cached review reasons | Public |

### 👑 Admin Governance (`/api/admin`)

| Method | Endpoint | Description | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/stats` | Global platform statistics & metrics | Admin |
| **GET** | `/api/admin/users` | List registered accounts (supports role filter) | Admin |
| **PATCH** | `/api/admin/users/{id:guid}/toggle-status` | Suspend or reactivate a user account | Admin |
| **GET** | `/api/admin/donations` | Admin view of all donations | Admin |

---

## 🧩 Types & Enums (Frontend Integration)

For seamless integration with frontend frameworks (React, Next.js, Vue, React Native, etc.), use the following TypeScript types, enums, and API response models.

### 1. 🏷️ Domain Enums

#### `UserRole`
```typescript
export enum UserRole {
  Admin = 1,
  Donor = 2,
  Charity = 3,
  Volunteer = 4
}
```

#### `UserType`
```typescript
export enum UserType {
  Individual = 1,
  Company = 2
}
```

#### `DonationStatus`
```typescript
export enum DonationStatus {
  Available = 0,
  Pending = 1,
  SearchingForVolunteer = 2,
  Accepted = 3,
  InTransit = 4,
  Delivered = 5,
  Cancelled = 6,
  Expired = 7
}
```

#### `MissionStatus`
```typescript
export enum MissionStatus {
  SearchingForVolunteer = 0,
  Accepted = 1,
  InTransit = 2,
  Delivered = 3,
  Cancelled = 4
}
```

#### `RequestStatus`
```typescript
export enum RequestStatus {
  Pending = 0,
  ApprovedByDonor = 1,
  Rejected = 2
}
```

#### `NotificationType`
```typescript
export enum NotificationType {
  MissionAvailable = 0,
  MissionAssigned = 1,
  MissionTaken = 2,
  DeliveryCompleted = 3,
  PenaltyApplied = 4,
  DonationStatusChanged = 5
}
```

#### `TransactionType`
```typescript
export enum TransactionType {
  MissionCompleted = 0,
  PointsRedeemed = 1,
  Penalty = 2
}
```

#### `ErrorType`
```typescript
export enum ErrorType {
  None = 0,
  Failure = 1,
  Validation = 2,
  NotFound = 3,
  Conflict = 4,
  Unauthorized = 5,
  Forbidden = 6
}
```

---

### 2. 📦 Generic API Response Envelope

All API endpoints return standard enveloped payloads:

```typescript
export interface ApiResult<T = any> {
  isSuccess: boolean;
  errorCode: string; // e.g. "SUCCESS", "VALIDATION_ERROR", "NOT_FOUND"
  data: T | null;
}

export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

---

### 3. 📝 Core TypeScript Models & DTOs

#### User & Authentication
```typescript
export interface RegisterRequestDto {
  fullName: string;
  email: string;
  phone: string;
  password: string;
  roleId: UserRole; // 1 = Admin, 2 = Donor, 3 = Charity, 4 = Volunteer
  countryId?: number | null;
  governorateId?: number | null;
  cityId?: number | null;
  addressDetails?: string | null;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string; // ISO 8601 string
}

export interface UserProfileDto {
  id: string; // GUID
  fullName: string;
  email: string;
  phone: string;
  roleId: UserRole;
  role: "Admin" | "Donor" | "Charity" | "Volunteer";
  userType: UserType;
  totalWalletPoints: number;
  trustScore: number;
  cityId?: number | null;
  vehicleType?: string | null;
  address?: string | null;
  addressDetails?: string | null;
  imagePath?: string | null;
  createdAt: string;
}

export interface PublicProfileDto {
  id: string; // GUID
  fullName: string;
  role: string;
  trustScore: number;
  totalWalletPoints: number;
  avatarUrl?: string | null;
  createdAt: string;
}
```

#### Donations & Requests
```typescript
export interface DonationResponseDto {
  id: string; // GUID
  categoryId?: number | null;
  title?: string | null;
  description?: string | null;
  foodType: string;
  quantity: string;
  weight?: number | null;
  expiryDate: string; // ISO 8601 string
  pickupLocation: string;
  cityId?: number | null;
  imageUrl?: string | null;
  status: string; // "Available" | "Pending" | "InTransit" | "Delivered" | "Cancelled" | "Expired"
  donorId: string;
  donorName: string;
  createdAt: string;
}

export interface CreateDonationDto {
  title?: string;
  description?: string;
  foodType: string;
  quantity: string;
  weight?: number;
  expiryDate: string;
  pickupLocation: string;
  cityId: number;
  categoryId: number;
  imageFile?: File | Blob;
}
```

#### Missions & Delivery Logistics
```typescript
export interface MissionDto {
  id: string; // GUID
  donationRequestId: string;
  volunteerId?: string | null;
  volunteerName?: string | null;
  missionStatus: string; // "SearchingForVolunteer" | "Accepted" | "InTransit" | "Delivered" | "Cancelled"
  pickupLocation: string;
  dropOffLocation: string;
  pickupTime?: string | null;
  deliveryTime?: string | null;
  createdAt: string;
  foodType?: string | null;
  quantity?: string | null;
}

export interface AcceptMissionResponseDto {
  missionId: string;
  status: "Accepted";
  pickupLocation: string;
}

export interface CompleteMissionRequestDto {
  otpCode: string;
}
```

#### Notifications & Reviews
```typescript
export interface NotificationDto {
  id: number;
  recipientId: string;
  title: string;
  body: string;
  type: string;
  isRead: boolean;
  createdAtUtc: string;
  relatedEntityId?: string | null;
}

export interface CreateReviewDto {
  missionId: string;
  revieweeId: string;
  reviewReasonId: number;
  rating?: number | null; // 1 - 5
  comment?: string | null;
}

export interface ReviewResponseDto {
  id: number;
  reviewerId: string;
  revieweeId: string;
  missionId: string;
  reviewReasonId: number;
  reviewReasonCode: string;
  isPositive: boolean;
  comment?: string | null;
  createdAt: string;
}
```

#### Dashboards & Gamification
```typescript
export interface DonorDashboardDto {
  totalDonationsCreated: number;
  totalItemsSaved: number;
  activePendingRequests: number;
  totalCompletedDonations: number;
  totalPoints: number;
  trustScore: number;
}

export interface VolunteerDashboardDto {
  completedDeliveries: number;
  totalPoints: number;
  activePenaltyWarnings: number;
  activeDeliveriesCount: number;
  trustScore: number;
}

export interface CharityDashboardDto {
  totalFoodReceived: number;
  pendingRequests: number;
  activeDeliveriesInTransit: number;
  approvedRequestsCount: number;
  trustScore: number;
}

export interface PointTransactionDto {
  id: number;
  amount: number;
  transactionType: string;
  referenceId?: string | null;
  createdAt: string;
}

export interface MyPointsResponseDto {
  totalPoints: number;
  trustScore: number;
  transactions: PagedResult<PointTransactionDto>;
}

export interface LeaderboardUserDto {
  rank: number;
  userId: string;
  fullName: string;
  avatarUrl?: string | null;
  role: string;
  totalPoints: number;
  trustScore: number;
  city?: string | null;
}
```

#### Lookups & Taxonomy
```typescript
export interface LookupDto {
  id: number;
  name: string;
  code?: string | null;
}

export interface ReviewReasonLookupDto {
  id: number;
  code: string;
  name: string;
  isPositive: boolean;
  fromRole: string;
  toRole: string;
}
```

---

### 4. 🛑 App Message & Error Codes (`AppMessageCodes`)

Use these standard strings for error handling and localization on the client:

| Domain | Error / Message Code | Description |
|---|---|---|
| **General** | `SUCCESS` | Operation completed successfully |
| | `INTERNAL_SERVER_ERROR` | An unhandled exception occurred |
| | `VALIDATION_ERROR` | Request payload failed validation rules |
| | `NOT_FOUND` | Requested entity does not exist |
| | `UNAUTHORIZED` | Bearer token missing or invalid |
| | `FORBIDDEN` | Insufficient role or access permissions |
| | `CONFLICT` / `CONCURRENCY_CONFLICT` | Resource already modified or locked |
| **Auth** | `LOGIN_SUCCESS` / `REGISTER_SUCCESS` | Authentication successful |
| | `INVALID_CREDENTIALS` | Invalid email or password |
| | `EMAIL_ALREADY_EXISTS` | An account with this email exists |
| | `TOKEN_EXPIRED` / `REFRESH_TOKEN_REVOKED` | Session is no longer valid |
| | `PASSWORD_TOO_WEAK` | Password does not meet security requirements |
| **Donations** | `DONATION_NOT_FOUND` | Specified donation ID was not found |
| | `DONATION_EXPIRED` | Listing has expired |
| | `DONATION_ALREADY_ACCEPTED` | Listing has already been claimed |
| | `DONATION_ALREADY_CANCELLED` | Listing is cancelled |
| **Missions** | `MISSION_NOT_FOUND` | Specified mission ID was not found |
| | `MISSION_ALREADY_TAKEN` | Another volunteer already accepted this mission |
| | `INVALID_OTP` / `OTP_EXPIRED` | Invalid or expired pickup OTP code |
| | `OTP_MAX_ATTEMPTS_EXCEEDED` | Too many failed OTP attempts |
| **Reviews** | `REVIEW_ALREADY_EXISTS` | User has already submitted a review for this mission |
| | `REVIEW_CANNOT_REVIEW_SELF` | Reviewer and reviewee cannot be identical |

---

## ⚙️ Configuration & Setup

### 1. Prerequisites
- **.NET SDK 10.0+**
- **MS SQL Server** (LocalDB, Docker, or Remote Instance)
- **Redis Server** (Local, Docker, or Managed Redis)
- **Cloudinary Account** (for media uploads)

### 2. appsettings.json Configuration

Configure your `EcoBridgeAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EcoBridgeDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "RedisConnection": "localhost:6379"
  },
  "JWT": {
    "SecretKey": "YOUR_STRONG_SECRET_KEY_FOR_HMACSHA256_SIGNING_AT_LEAST_32_CHARS",
    "Issuer": "Eco-Bridge-API",
    "Audience": "Eco-Bridge-Clients",
    "DurationInMinutes": 30,
    "RefreshTokenDurationInDays": 7
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUDINARY_CLOUD_NAME",
    "ApiKey": "YOUR_CLOUDINARY_API_KEY",
    "ApiSecret": "YOUR_CLOUDINARY_API_SECRET"
  },
  "AllowedOrigins": "*"
}
```

### 3. Database Migrations

Apply the latest Entity Framework Core migrations to create or update the SQL Server database:

```bash
# Apply EF Core Migrations
dotnet ef database update --project Infrastructure --startup-project EcoBridgeAPI
```

### 4. Running the Application

Run the API locally in development mode:

```bash
dotnet run --project EcoBridgeAPI
```

- **Swagger Documentation**: `https://localhost:<port>/swagger`
- **OpenAPI v1 Spec**: `https://localhost:<port>/openapi/v1.json`
- **SignalR Real-Time Hub**: `https://localhost:<port>/hubs/missions`

---

## 🧪 Testing & Verification

Run the solution build and tests:

```bash
# Build the entire solution
dotnet build EcoBridge_Project.slnx
```

