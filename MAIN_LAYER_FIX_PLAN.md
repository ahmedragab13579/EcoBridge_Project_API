# Main Application Layer Scan: Fix Plan

## Scope

This scan covered the runtime layers only: `Domain`, `Application`, `Infrastructure`, and `EcoBridgeAPI`. The `tests` directory and generated `bin`/`obj` output were excluded. The plan focuses on confirmed flow defects and high-risk operational issues found in the current code.

## Priority 0: Security and configuration

### 1. Revoke and remove committed credentials

- **Affected area:** `EcoBridgeAPI/appsettings.json`
- Database credentials, JWT signing material, Cloudinary credentials, Brevo API key, and Redis credentials are stored in source control.
- Revoke or rotate every exposed credential before deployment.
- Replace values with environment variables, user secrets for local development, or the deployment secret store.
- Add a sanitized configuration template and secret-scanning/pre-commit or CI protection.
- Ensure production startup fails when required secrets are missing rather than falling back to unsafe defaults.

### 2. Make cookie authentication and CORS contracts consistent

- **Affected areas:** `EcoBridgeAPI/Controllers/BaseApiController.cs`, `EcoBridgeAPI/Controllers/AuthController.cs`, `Infrastructure/Services/Implementation/CookieService.cs`, `EcoBridgeAPI/Program.cs`
- The refresh endpoint sends `AccessToken` from an `accessToken` cookie, but `CookieService` writes only `refreshToken` and `csrfToken`. The access token is also passed as the CSRF cookie value during login and refresh.
- Choose one deliberate design: either store the access token in an HttpOnly cookie, or return/use the access token through the documented header flow. Then make refresh, logout, revoke, and CSRF validation use the same contract.
- Set cookie expiry from the refresh-token lifetime where appropriate, and document the cookie paths and credential requirements.
- Replace wildcard CORS with explicit trusted origins whenever credentialed cookies are used; call `AllowCredentials()` only with explicit origins.
- Add end-to-end checks for login, refresh, revoke, logout, expired access tokens, and cross-origin browser requests.

## Priority 1: Broken business flows

### 3. Handle Identity registration failure before issuing tokens

- **Affected area:** `Application/Features/Auth/Handlers/RegisterCommandHandler.cs`
- Check `IdentityResult.Succeeded` immediately after `CreateAsync`.
- Map identity errors to the project error envelope and stop processing on failure.
- Issue tokens only after successful persistence; consider a transaction if account creation also creates related records.
- Verify duplicate email, invalid password, and database failure paths.

### 4. Enable the FluentValidation pipeline

- **Affected areas:** validators under `Application/Features/**/Validations`, MediatR registration in `EcoBridgeAPI/Program.cs`
- Register validators from the Application assembly.
- Add a MediatR validation behavior, or use the project’s established equivalent, so validators execute before handlers.
- Confirm null request handling, error-code preservation, and consistent HTTP 400 responses.
- Cover all externally reachable commands, not only login.

### 5. Enforce volunteer eligibility during assignment

- **Affected area:** `Application/Features/Donation/Commands/AssignVolunteerCommand.cs`
- Require the target account to be a volunteer, active, and available.
- Validate that non-admin callers can assign only themselves and that admin target IDs are valid.
- Atomically mark the volunteer unavailable when assignment succeeds.
- Add a database/concurrency guard so two assignments cannot claim the same volunteer or donation simultaneously.

### 6. Repair charity unclaim after volunteer assignment

- **Affected areas:** `Application/Features/Donation/Handler/UnClaimCommandHandler.cs`, `Domain/Models/Donation/Donation.Lifecycle.cs`
- The handler accepts `VolunteerAssigned`, but calls `Donation.Unclaim()`, which accepts only `SearchingForVolunteer` and therefore rejects the intended flow.
- Add a domain transition for assigned-donation release, or use the existing `ReturnToVolunteerSearch()` plus the required charity release transition, with explicit rules for volunteer compensation and availability.
- Persist the transition before publishing notifications and SignalR events.
- Verify both simple unclaim and assigned unclaim, including rollback when persistence fails.

### 7. Restrict reviews to completed deliveries

- **Affected area:** `Application/Features/Reviews/Handlers/CreateReviewCommandHandler.cs`
- Require the mission to be delivered/completed and not cancelled before accepting a review.
- Confirm the donation and mission state are consistent and that the reviewer/reviewee pair is allowed by the workflow.
- Keep duplicate-review protection at both handler and database constraint levels where possible.

### 8. Base volunteer timeout on assignment time

- **Affected area:** `Application/BackgroundJobs/VolunteerTimeoutJob.cs`
- Add or use an explicit volunteer-assigned timestamp on the donation/mission.
- Select only missions whose assignment timestamp is older than the timeout threshold.
- Clear or update the timestamp on reassignment, pickup, cancellation, and completion.
- Add a regression case for donations that waited in charity/search states before being assigned.

## Priority 2: Consistency and reliability

### 9. Choose one refresh-token persistence model

- **Affected areas:** `Application/Services/RedisRefreshTokenService.cs`, `Application/BackgroundJobs/TransientDataCleanupJob.cs`, EF refresh-token model/context
- The active flow stores tokens in Redis while cleanup targets the EF `RefreshTokens` table, so database cleanup and auditing do not affect active tokens.
- Either remove the unused EF flow and make Redis cleanup explicit, or move the active flow to the EF model with proper revocation and indexing.
- Document the choice, expiration semantics, revoke-all behavior, and failure behavior.

### 10. Make refresh-token rotation and revoke-all concurrency-safe

- **Affected area:** `Application/Services/RedisRefreshTokenService.cs`
- The read/append/write list update can lose tokens under concurrent login or refresh requests.
- Use atomic Redis data structures/operations, a transaction/Lua script, or a per-user lock with bounded expiry.
- Make rotation single-use and handle concurrent refresh attempts deterministically.
- Decide whether Redis outages fail closed for refresh while returning a controlled error instead of an unhandled 500.

### 11. Add durable post-commit event delivery

- **Affected areas:** handlers that save and then publish notifications, SignalR events, or cache invalidations
- Current post-save side effects can be lost after the database commit.
- Introduce an outbox table and dispatcher, or an equivalent durable retry mechanism.
- Record event delivery failures, retry with backoff, and make consumers idempotent.
- Apply this first to delivery completion, pickup confirmation, unclaim, notifications, and cache invalidation.

### 12. Remove duplicate delivery transition

- **Affected area:** `Application/Features/Missions/Handlers/CompleteMissionCommandHandler.cs`
- Call `RecordDelivery()` once before completing the donation.
- Keep the operation idempotent through an explicit state guard rather than duplicate invocation.
- Verify OTP failure still persists the attempt count and successful completion awards points once.

### 13. Protect the Hangfire dashboard explicitly

- **Affected area:** Hangfire mapping in `EcoBridgeAPI/Program.cs`
- Add an explicit dashboard authorization filter/policy restricted to administrators or an internal network.
- Verify behavior behind a reverse proxy and ensure no production default permits anonymous access.

## Recommended implementation order

1. Rotate exposed secrets and remove them from configuration/source control.
2. Repair the cookie, refresh, CSRF, and CORS contract.
3. Fix registration result handling and enable command validation.
4. Correct volunteer assignment and assigned unclaim domain transitions.
5. Fix review eligibility and timeout timestamps.
6. Consolidate refresh-token persistence and make rotation atomic.
7. Remove duplicate delivery transition and secure Hangfire.
8. Introduce outbox-based delivery for post-commit events.

## Verification gate

- Build all four runtime projects without compiling `tests` as part of the scan.
- Exercise the authentication lifecycle with cookies and expired access tokens.
- Exercise donation claim, volunteer assignment, assigned unclaim, pickup, completion, cancellation, and timeout flows.
- Verify authorization for donor, charity, volunteer, and admin roles.
- Confirm invalid command payloads return the standard validation envelope.
- Run concurrency checks for assignment and refresh-token rotation.
- Confirm no secrets remain in tracked configuration and that production configuration fails clearly when required values are absent.