# EcoBridge API

EcoBridge is a food-rescue and community logistics platform that connects donors, charities, and volunteers to reduce food waste and support vulnerable communities. The system is designed as a modern, domain-driven API with clear separation of concerns and a workflow that moves from food availability to secure handoff and community feedback.

This project follows a Clean Architecture approach with a strong Domain layer and application workflows designed around real-world operational rules.

---

## Overview

EcoBridge helps:
- Donors publish surplus food for collection
- Charities request and receive donations
- Volunteers accept and complete delivery missions
- Admins monitor platform health, trust, and account activity
- The system calculates trust, points, and penalties based on behavior

The solution is built around:
- .NET 10 / ASP.NET Core
- Entity Framework Core
- ASP.NET Identity
- SQL Server
- Redis
- SignalR
- JWT authentication
- Domain-driven business rules

---

## Expected Project Structure

```text
EcoBridge_Project/
├── EcoBridge.API/                 # API layer, controllers, middleware, auth, Swagger
├── EcoBridge.Application/         # Commands, queries, DTOs, validation, handlers
├── EcoBridge.Domain/              # Aggregates, entities, enums, value rules, invariants
├── EcoBridge.Infrastructure/      # EF Core, Identity, repositories, external integrations
├── EcoBridge.Shared/              # Cross-cutting utilities, messages, constants
├── EcoBridge.Tests/               # Unit/integration tests
├── README.md
├── EcoBridge_Project.sln
└── appsettings.json
```

---

## Architectural Direction

### Domain-first design
The Domain layer is the business heart of the system. It owns:
- Account and role rules
- Donation lifecycle states
- Mission lifecycle and completion rules
- OTP validation and mission security
- Trust score and points logic
- Review and penalty rules
- Geographic and lookup validation

### Application layer
The Application layer coordinates:
- Commands and queries
- Validation and authorization
- Use-case orchestration
- Integration with Infrastructure services
- Domain event handling

### Infrastructure layer
Infrastructure handles:
- EF Core persistence
- Identity and authentication
- Redis cache
- Background jobs
- Notifications and external integrations
- Media storage

### API layer
The API layer is responsible for:
- HTTP endpoints
- Request/response contracts
- Authentication and authorization
- Swagger/OpenAPI documentation
- Real-time communication via SignalR

---

## Domain Model Summary

### Core entities

- Account
  - Base identity entity for all users
  - Holds profile, wallet, trust score, role, status, and account metadata
  - Supports donor, charity, volunteer, and admin role behavior

- Donation
  - Represents a food listing or surplus item
  - Has lifecycle states such as Available, Pending, Accepted, InTransit, Delivered, Cancelled, Expired
  - Contains donor ownership, optional charity/volunteer assignment, and cancellation context

- DeliveryMission
  - Created from a donation assignment flow
  - Coordinates logistics between charity and volunteer
  - Handles OTP verification and mission completion

- Review
  - Stores post-mission feedback
  - Links reviewer and reviewee
  - Supports trust and behavior scoring

- Notification
  - Platform message records for user updates
  - Used for mission updates, trust warnings, assignment events, and administrative communication

- PointTransaction
  - Records points earned, spent, or deducted
  - Supports gamification and platform engagement

- PenaltyLog
  - Stores any trust or behavior penalties for users
  - Used to maintain account quality and fairness

- RefreshToken
  - Secure token storage for authentication renewal flows

- Category
  - Donation categorization (food type, packing type, etc.)

- Country / Governorate / City
  - Geographic hierarchy used for location-based matching and mission assignment

- ReviewReason
  - Standard reasons used for positive or negative reviews

---

## Expected User Workflow

### 1. User registration and onboarding
A user registers as one of:
- Donor
- Charity
- Volunteer
- Admin

The account is created with:
- Full name
- Email and phone
- Role
- Optional geographic data
- Wallet and trust score defaults
- Status flags and security settings

Expected flow:
1. User submits registration details
2. App validates password, email, and role
3. Account is created
4. JWT token and refresh token are issued
5. User completes profile setup if needed

---

### 2. Donor creates a donation
A donor posts surplus food to the platform.

Expected flow:
1. Donor logs in
2. Donor fills donation information:
   - title/description
   - category
   - quantity/weight
   - pickup location
   - expiry date
   - optional image
3. Donation is created in Available state
4. Donation becomes visible to charities and volunteers depending on permissions
5. Donation is kept in a valid state until accepted, cancelled, or expired

---

### 3. Charity requests or claims donation
A charity discovers a donation and requests it.

Expected flow:
1. Charity queries open donations
2. Charity selects a donation
3. Charity creates a donation request
4. Donor can approve or reject
5. If approved, the donation moves to the next lifecycle step
6. A mission is created or prepared for assignment

---

### 4. Volunteer mission assignment
The system matches a delivery mission to a volunteer.

Expected flow:
1. Volunteer sees available missions
2. Volunteer accepts a mission
3. Mission status moves to Accepted / InTransit
4. Pickup and delivery steps are tracked
5. System maintains assignment and timeout protection

---

### 5. Pick-up and delivery validation
To protect the handoff and reduce fraud, the mission uses delivery verification.

Expected flow:
1. Volunteer reaches the donor pickup point
2. Donor confirms pickup
3. Mission enters in-transit state
4. Volunteer moves to final delivery point
5. OTP is validated
6. Mission is marked Delivered
7. Related reward and trust logic is triggered

---

### 6. Review and trust update
After successful operation, participants review each other.

Expected flow:
1. Review is created for a completed mission
2. Reason and rating are validated
3. The system updates:
   - trust score
   - points
   - platform reputation
4. Notification is sent to relevant users

---

### 7. Penalties and account health
If a user misbehaves or fails to complete a mission, the system applies consequences.

Examples:
- Missing a mission
- Cancelled mission without valid reason
- Abandoned delivery
- Fraudulent OTP attempts
- Repeated bad behavior

Expected flow:
1. Rule is triggered
2. Penalty is logged
3. Trust score is reduced
4. Appropriate notification is generated
5. Repeated penalties may lead to restricted access

---

### 8. Admin oversight
Admins monitor user behavior and platform health.

Admin responsibilities:
- Review user account status
- Approve or reject charity verification flows
- Monitor donation and mission activity
- Review system warnings and trust issues
- Manage platform-wide metrics

---

## Business Rules and Expected Domain Behavior

### Account rules
- Account must be active to perform sensitive actions
- Deleted accounts cannot perform operations
- Role-specific methods should be validated
- Trust score and wallet operations must respect minimum and maximum values
- Role transitions should be explicitly controlled by business rules

### Donation rules
- Donation cannot be claimed if already assigned
- Donation cannot be cancelled in terminal states
- Expired donations are processed by background jobs
- Deletion should not leave active assignment inconsistencies
- Validation should cover required fields, lengths, and status transitions

### Mission rules
- Mission is only valid for active donation and active account states
- Volunteers must be eligible to claim
- OTP must be valid and not expired
- Failed OTP attempts should be counted and limited
- Mission states should be consistent and terminal when completed or cancelled

### Review rules
- Reviews should be restricted to mission participants
- Duplicate reviews should be prevented
- Review reason codes should align with role semantics
- Review content should be normalized and validated

---

## Expected API Modules

### Authentication
- Register
- Login
- Refresh token
- Revoke token
- Get current user
- Change password

### Users and profiles
- Get profile
- Update profile
- Public profile lookup
- Role-aware data loading

### Donations
- Create donation
- List open donations
- Get donation details
- Update donation
- Cancel donation
- Delete donation softly

### Requests and missions
- Submit donation request
- Approve/deny request
- Accept mission
- Confirm pickup
- Complete mission
- Validate OTP
- Mission history

### Reviews and notifications
- Submit review
- Get user notifications
- Mark as read
- Review metrics

### Admin
- User status toggling
- Platform stats
- Donation monitoring
- Mission monitoring
- Trust and penalty review

---

## Expected Workflow Diagram

```text
Donor
  ↓
Create Donation
  ↓
Donation Available
  ↓
Charity Requests / Approves
  ↓
Volunteer Accepts Mission
  ↓
Pickup / In Transit
  ↓
Delivery Confirmation
  ↓
OTP Validation
  ↓
Mission Delivered
  ↓
Trust + Points + Review
  ↓
Notification + Analytics
```

---

## Data Concepts and Platform Logic

### Wallet and trust
The platform calculates:
- wallet points earned from successful completion
- trust score based on completed missions and behavior
- penalties based on cancellations and failed operations

This creates a community reputation system that rewards reliable participation.

### Location matching
Donations and missions use:
- Country
- Governorate
- City
- Optional address details

This supports:
- local matching
- fair mission assignment
- efficient volunteer distribution

### Event-driven behavior
The platform is designed to generate:
- notification events
- mission assignment events
- donation status changes
- trust updates
- review events
- penalty logs

These can be used by background jobs and real-time client updates.

---

## Quality Goals

The project aims to provide:
- Safe domain behavior with invariants
- Role-based access control
- A clean separation between business rules and persistence
- Reliable mission and donation lifecycle tracking
- Human-friendly and machine-readable API contracts
- Maintainable code structure for future growth

---

## Setup

### Prerequisites
- .NET 10 SDK
- SQL Server
- Redis
- Optional cloud media provider for images

### Restore and run
```bash
dotnet restore
dotnet build
dotnet run --project EcoBridge.API
```

### Database
```bash
dotnet ef database update --project EcoBridge.Infrastructure --startup-project EcoBridge.API
```

### Swagger
Open:
```text
https://localhost:<port>/swagger
```

---

## Current Direction

The current codebase is moving toward a strong, business-rule-first model where:
- Domain entities control valid state transitions
- Application services orchestrate workflows
- Infrastructure persists and integrates external systems
- API exposes a clean, standardized experience for clients

This structure is the right foundation for the food-rescue marketplace described by the project.

---

## Final Note

EcoBridge is designed as a real-world logistics and community-impact platform, not just a CRUD app. Its expected behavior is intentionally workflow-driven:
- donations are created and validated
- charities request and receive food
- volunteers complete transport safely
- trust and points reward good behavior
- admins maintain platform integrity

This README reflects the expected shape and workflow of the platform as it is designed to operate.