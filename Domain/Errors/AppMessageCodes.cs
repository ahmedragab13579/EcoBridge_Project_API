namespace EcoBridge.Domain.Errors;

public static class AppMessageCodes
{
    // ==========================================
    // 1. General & System Codes
    // ==========================================
    public const string SUCCESS = "SUCCESS";
    public const string IMAGE_UPLOAD_FAILED = "IMAGE_UPLOAD_FAILED";
    public const string HANGFIRE_JOB_ID_TOO_LONG = "HANGFIRE_JOB_ID_TOO_LONG";
    public const string PUBLIC_ID_REQUIRED = "PUBLIC_ID_REQUIRED";
    public const string USER_ID_REQUIRED = "USER_ID_REQUIRED";
    public const string REQUEST_BODY_REQUIERD = "REQUEST_BODY_REQUIERD";
    // ===== Common / Cross-cutting =====

    // ===== Auth: Login =====
    public const string EMAIL_REQUIERD = "EMAIL_REQUIERD";
    public const string EMAIL_INVALID_FORMAT = "EMAIL_INVALID_FORMAT";
    public const string EMAIL_TOO_LONG = "EMAIL_TOO_LONG";
    public const string PASSWORD_REQUIERD = "PASSWORD_REQUIERD";
    public const string PASSWORD_TOO_SHORT = "PASSWORD_TOO_SHORT";
    public const string PASSWORD_TOO_LONG = "PASSWORD_TOO_LONG";

    // ===== Auth: Change Password =====
    public const string CURRENT_PASSWORD_REQUIERD = "CURRENT_PASSWORD_REQUIERD";
    public const string NEW_PASSWORD_REQUIERD = "NEW_PASSWORD_REQUIERD";
    public const string NEW_PASSWORD_TOO_SHORT = "NEW_PASSWORD_TOO_SHORT";
    public const string NEW_PASSWORD_TOO_LONG = "NEW_PASSWORD_TOO_LONG";
    public const string NEW_PASSWORD_SAME_AS_CURRENT = "NEW_PASSWORD_SAME_AS_CURRENT";

    // ===== Auth: Refresh Token =====
    public const string REFRESH_TOKEN_REQUIERD = "REFRESH_TOKEN_REQUIERD";
    public const string ACCESS_TOKEN_REQUIERD = "ACCESS_TOKEN_REQUIERD";

    // ===== Auth: Revoke Token =====
    public const string REVOKE_TARGET_REQUIERD = "REVOKE_TARGET_REQUIERD";

    // ===== Auth: Register =====
    public const string FULL_NAME_REQUIERD = "FULL_NAME_REQUIERD";
    public const string FULL_NAME_TOO_LONG = "FULL_NAME_TOO_LONG";
    public const string PHONE_REQUIERD = "PHONE_REQUIERD";
    public const string PHONE_TOO_LONG = "PHONE_TOO_LONG";
    public const string ROLE_REQUIERD = "ROLE_REQUIERD";
    public const string ROLE_INVALID = "ROLE_INVALID";
    public const string CITY_ID_INVALID = "CITY_ID_INVALID";

    // ===== User Profile Update =====
    public const string ADDRESS_DETAILS_TOO_LONG = "ADDRESS_DETAILS_TOO_LONG";
    public const string ADDRESS_TOO_LONG = "ADDRESS_TOO_LONG";
    public const string VEHICLE_TYPE_TOO_LONG = "VEHICLE_TYPE_TOO_LONG";
    public const string VEHICLE_DETAILS_TOO_LONG = "VEHICLE_DETAILS_TOO_LONG";
    public const string ORGANIZATION_NAME_TOO_LONG = "ORGANIZATION_NAME_TOO_LONG";
    public const string REGISTRATION_NUMBER_TOO_LONG = "REGISTRATION_NUMBER_TOO_LONG";
    public const string PROFILE_IMAGE_TOO_LARGE = "PROFILE_IMAGE_TOO_LARGE";

    // ===== Pagination (Admin queries) =====
    public const string PAGE_INVALID = "PAGE_INVALID";
    public const string PAGE_SIZE_INVALID = "PAGE_SIZE_INVALID";
    public const string DONATION_REQUEST_CREATED = "DONATION_REQUEST_CREATED";
    public const string DELETE_FAILED = "DELETE_FAILED";
    public const string DONATION_NOT_CANCELLABLE_IN_CURRENT_STATUS = "DONATION_NOT_CANCELLABLE_IN_CURRENT_STATUS";
    public const string DONATION_CANCELLATION_REASON_REQUIRED = "DONATION_CANCELLATION_REASON_REQUIRED";
    public const string DONATION_CANCELLATION_REASON_TOO_LONG = "DONATION_CANCELLATION_REASON_TOO_LONG";
    public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
    public const string INVALID_EMAIL_FORMAT = "INVALID_EMAIL_FORMAT";
    public const string INVALID_PHONE_FORMAT = "INVALID_PHONE_FORMAT";
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string ORGANIZATION_INFO_REQUIRED = "ORGANIZATION_INFO_REQUIRED";
    public const string TRUST_SCORE_LIMIT_EXCEEDED = "TRUST_SCORE_LIMIT_EXCEEDED";
    public const string NOT_FOUND = "NOT_FOUND";
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string CONFLICT = "CONFLICT";
    public const string CONCURRENCY_CONFLICT = "CONCURRENCY_CONFLICT";
    public const string INVALID_INPUT = "INVALID_INPUT";
    public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";
    public const string TOO_MANY_REQUESTS = "TOO_MANY_REQUESTS";

    // ==========================================
    // 2. File & Cloudinary Storage Codes
    // ==========================================
    public const string FILE_REQUIRED = "FILE_REQUIRED";
    public const string FILE_TOO_LARGE = "FILE_TOO_LARGE";
    public const string INVALID_FILE_TYPE = "INVALID_FILE_TYPE";
    public const string IMAGE_PATH_TOO_LONG = "IMAGE_PATH_TOO_LONG";
    public const string UPLOAD_FAILED = "UPLOAD_FAILED";
    public const string UPLOAD_SUCCESS = "UPLOAD_SUCCESS";
    public const string UPDATE_SUCCESS = "UPDATE_SUCCESS";

    // ==========================================
    // 3. Authentication & User Account Codes
    // ==========================================
    public const string LOGIN_SUCCESS = "LOGIN_SUCCESS";
    public const string REGISTER_SUCCESS = "REGISTER_SUCCESS";
    public const string NOT_SUPPORT_REGEST_ADMIN = "NOT_SUPPORT_REGEST_ADMIN";
    public const string LOGOUT_SUCCESS = "LOGOUT_SUCCESS";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string PHONE_ALREADY_EXISTS = "PHONE_ALREADY_EXISTS";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string ACCOUNT_NOT_FOUND = "ACCOUNT_NOT_FOUND";
    public const string ACCOUNT_DEACTIVATED = "ACCOUNT_DEACTIVATED";
    public const string ACCOUNT_SUSPENDED = "ACCOUNT_SUSPENDED";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string REFRESH_TOKEN_REVOKED = "REFRESH_TOKEN_REVOKED";
    public const string PASSWORD_MISMATCH = "PASSWORD_MISMATCH";
    public const string PASSWORD_INCORRECT = "PASSWORD_INCORRECT";
    public const string PASSWORD_TOO_WEAK = "PASSWORD_TOO_WEAK";
    public const string PASSWORD_CHANGE_SUCCESS = "PASSWORD_CHANGE_SUCCESS";
    public const string ME_SUCCESS = "ME_SUCCESS";
    public const string PROFILE_RETRIEVED = "PROFILE_RETRIEVED";
    public const string PROFILE_UPDATED = "PROFILE_UPDATED";
    public const string USER_STATUS_UPDATED = "USER_STATUS_UPDATED";
    public const string USERS_RETRIEVED = "USERS_RETRIEVED";
    public const string PASSWORD_RESET_EMAIL_SENT = "PASSWORD_RESET_EMAIL_SENT";
    public const string PASSWORD_RESET_SUCCESS = "PASSWORD_RESET_SUCCESS";
    public const string EMAIL_VERIFIED_SUCCESS = "EMAIL_VERIFIED_SUCCESS";
    public const string PUBLIC_PROFILE_RETRIEVED = "PUBLIC_PROFILE_RETRIEVED";
    public const string CATEGORIES_RETRIEVED = "CATEGORIES_RETRIEVED";
    public const string POINTS_RETRIEVED = "POINTS_RETRIEVED";
    public const string LEADERBOARD_RETRIEVED = "LEADERBOARD_RETRIEVED";
    public const string DASHBOARD_RETRIEVED = "DASHBOARD_RETRIEVED";
    public const string INVALID_ROLE = "INVALID_ROLE";
    public const string INVALID_USER_TYPE = "INVALID_USER_TYPE";                                  // ✅ NEW

    // ==========================================
    // 4. Actor Roles (Donor, Charity, Volunteer)
    // ==========================================
    public const string DONOR_NOT_FOUND = "DONOR_NOT_FOUND";
    public const string CHARITY_NOT_FOUND = "CHARITY_NOT_FOUND";
    public const string CHARITY_NOT_VERIFIED = "CHARITY_NOT_VERIFIED";
    public const string VOLUNTEER_NOT_FOUND = "VOLUNTEER_NOT_FOUND";
    public const string VOLUNTEER_INACTIVE = "VOLUNTEER_INACTIVE";
    public const string VOLUNTEER_FETCHED = "VOLUNTEER_FETCHED";
    public const string VOLUNTEER_CREATED = "VOLUNTEER_CREATED";
    public const string VOLUNTEER_UPDATED = "VOLUNTEER_UPDATED";
    public const string VOLUNTEER_DELETED = "VOLUNTEER_DELETED";
    public const string VOLUNTEER_ASSIGNED = "VOLUNTEER_ASSIGNED";
    public const string VOLUNTEER_AVAILABILITY_UPDATED = "VOLUNTEER_AVAILABILITY_UPDATED";

    // ==========================================
    // 5. Account (Domain) Codes
    // ==========================================
    // ── Account: required fields ─────────────────────────────────
    public const string FULL_NAME_REQUIRED = "FULL_NAME_REQUIRED";                                // ✅ NEW
    public const string EMAIL_REQUIRED = "EMAIL_REQUIRED";                                        // ✅ NEW
    public const string PHONE_REQUIRED = "PHONE_REQUIRED";                                        // ✅ NEW
    public const string PASSWORD_HASH_REQUIRED = "PASSWORD_HASH_REQUIRED";                        // ✅ NEW
    public const string IMAGE_PATH_REQUIRED = "IMAGE_PATH_REQUIRED";                              // ✅ NEW

    // ── Account: invalid values ──────────────────────────────────
    public const string INVALID_CITY_ID = "INVALID_CITY_ID";                                      // ✅ NEW
    public const string INVALID_POINTS_AMOUNT = "INVALID_POINTS_AMOUNT";                          // ✅ NEW
    public const string INVALID_TRUST_SCORE_AMOUNT = "INVALID_TRUST_SCORE_AMOUNT";                // ✅ NEW

    // ── Account: role/type compatibility ─────────────────────────
    public const string ADMIN_CANNOT_BE_COMPANY = "ADMIN_CANNOT_BE_COMPANY";                      // ✅ NEW
    public const string CHARITY_MUST_BE_COMPANY = "CHARITY_MUST_BE_COMPANY";                      // ✅ NEW

    // ── Account: wallet ──────────────────────────────────────────
    public const string WALLET_POINTS_LIMIT_EXCEEDED = "WALLET_POINTS_LIMIT_EXCEEDED";            // ✅ NEW
    // INSUFFICIENT_POINTS already defined in section 9

    // ── Account: state / lifecycle ───────────────────────────────
    public const string ACCOUNT_DELETED = "ACCOUNT_DELETED";                                      // ✅ NEW
    // ACCOUNT_DEACTIVATED already defined in section 3

    // ── Account: role-gated operations ───────────────────────────
    public const string CHARITY_ONLY_OPERATION = "CHARITY_ONLY_OPERATION";                        // ✅ NEW
    public const string VOLUNTEER_ONLY_OPERATION = "VOLUNTEER_ONLY_OPERATION";                    // ✅ NEW

    // ==========================================
    // 6. Category (Domain) Codes
    // ==========================================
    public const string CATEGORY_NAME_AR_REQUIRED = "CATEGORY_NAME_AR_REQUIRED";                  // ✅ NEW
    public const string CATEGORY_NAME_EN_REQUIRED = "CATEGORY_NAME_EN_REQUIRED";                  // ✅ NEW
    public const string CATEGORY_NAME_AR_TOO_LONG = "CATEGORY_NAME_AR_TOO_LONG";                  // ✅ NEW
    public const string CATEGORY_NAME_EN_TOO_LONG = "CATEGORY_NAME_EN_TOO_LONG";                  // ✅ NEW

    // ==========================================
    // 7. Donation (Domain) Codes
    // ==========================================
    // ── Donation: required fields ────────────────────────────────
    public const string DONOR_ID_REQUIRED = "DONOR_ID_REQUIRED";                                  // ✅ NEW
    public const string CHARITY_ID_REQUIRED = "CHARITY_ID_REQUIRED";                              // ✅ NEW
    public const string VOLUNTEER_ID_REQUIRED = "VOLUNTEER_ID_REQUIRED";                          // ✅ NEW
    public const string FOOD_TYPE_REQUIRED = "FOOD_TYPE_REQUIRED";                                // ✅ NEW
    public const string QUANTITY_REQUIRED = "QUANTITY_REQUIRED";                                  // ✅ NEW
    public const string PICKUP_LOCATION_REQUIRED = "PICKUP_LOCATION_REQUIRED";                    // ✅ NEW
    public const string CANCELLATION_REASON_REQUIRED = "CANCELLATION_REASON_REQUIRED";            // ✅ NEW

    // ── Donation: length limits ──────────────────────────────────
    public const string FOOD_TYPE_TOO_LONG = "FOOD_TYPE_TOO_LONG";                                // ✅ NEW
    public const string QUANTITY_TOO_LONG = "QUANTITY_TOO_LONG";                                  // ✅ NEW
    public const string PICKUP_LOCATION_TOO_LONG = "PICKUP_LOCATION_TOO_LONG";                    // ✅ NEW
    public const string TITLE_TOO_LONG = "TITLE_TOO_LONG";                                        // ✅ NEW
    public const string DESCRIPTION_TOO_LONG = "DESCRIPTION_TOO_LONG";                            // ✅ NEW
    public const string CANCELLATION_REASON_TOO_LONG = "CANCELLATION_REASON_TOO_LONG";            // ✅ NEW

    // ── Donation: numeric / identity validation ──────────────────
    public const string INVALID_CATEGORY_ID = "INVALID_CATEGORY_ID";                              // ✅ NEW
    public const string INVALID_WEIGHT = "INVALID_WEIGHT";                                        // ✅ NEW
    public const string WEIGHT_EXCEEDS_LIMIT = "WEIGHT_EXCEEDS_LIMIT";                            // ✅ NEW

    // ── Donation: state conflicts ────────────────────────────────
    public const string DONATION_INVALID_STATUS = "DONATION_INVALID_STATUS";                      // already existed
    public const string DONATION_ALREADY_SEARCHING_FOR_VOLUNTEER = "DONATION_ALREADY_SEARCHING_FOR_VOLUNTEER"; // already existed
    public const string DONATION_NOT_EDITABLE_IN_CURRENT_STATUS = "DONATION_NOT_EDITABLE_IN_CURRENT_STATUS";   // ✅ NEW
    public const string DONATION_NOT_ACCEPTED = "DONATION_NOT_ACCEPTED";                          // ✅ NEW
    public const string DONATION_NOT_SEARCHING_FOR_VOLUNTEER = "DONATION_NOT_SEARCHING_FOR_VOLUNTEER"; // ✅ NEW
    public const string DONATION_NOT_VOLUNTEER_ASSIGNED = "DONATION_NOT_VOLUNTEER_ASSIGNED";      // ✅ NEW
    public const string DONATION_ALREADY_HAS_ACTIVE_MISSION = "DONATION_ALREADY_HAS_ACTIVE_MISSION"; // ✅ NEW
    public const string DONATION_HAS_ACTIVE_MISSION = "DONATION_HAS_ACTIVE_MISSION";              // ✅ NEW
    public const string DONATION_NO_ACTIVE_MISSION = "DONATION_NO_ACTIVE_MISSION";                // ✅ NEW
    public const string DONATION_NOT_IN_TRANSIT = "DONATION_NOT_IN_TRANSIT";                      // ✅ NEW
    public const string DONATION_NOT_CANCELLABLE = "DONATION_NOT_CANCELLABLE";                    // ✅ NEW
    public const string DONATION_NOT_EXPIRABLE_IN_CURRENT_STATUS = "DONATION_NOT_EXPIRABLE_IN_CURRENT_STATUS"; // ✅ NEW

    // ── Donation: existing legacy codes ──────────────────────────
    public const string DONATION_CREATED = "DONATION_CREATED";
    public const string DONATION_UPDATED = "DONATION_UPDATED";
    public const string DONATION_DELETED = "DONATION_DELETED";
    public const string DONATION_RETRIEVED = "DONATION_RETRIEVED";
    public const string DONATIONS_RETRIEVED = "DONATIONS_RETRIEVED";
    public const string DONATION_NOT_FOUND = "DONATION_NOT_FOUND";
    public const string DONATION_EXPIRED = "DONATION_EXPIRED";
    public const string DONATION_ALREADY_REQUESTED = "DONATION_ALREADY_REQUESTED";
    public const string DONATION_ALREADY_ACCEPTED = "DONATION_ALREADY_ACCEPTED";
    public const string DONATION_ALREADY_COMPLETED = "DONATION_ALREADY_COMPLETED";
    public const string DONATION_ALREADY_CANCELLED = "DONATION_ALREADY_CANCELLED";
    public const string DONATION_STATUS_UPDATED = "DONATION_STATUS_UPDATED";
    public const string DONATION_CANCELLED = "DONATION_CANCELLED";
    public const string DONATION_ACCEPTED = "DONATION_ACCEPTED";
    public const string DONATION_CANNOT_BE_MODIFIED = "DONATION_CANNOT_BE_MODIFIED";
    public const string INVALID_DONATION_QUANTITY = "INVALID_DONATION_QUANTITY";
    public const string INVALID_DONATION_CATEGORY = "INVALID_DONATION_CATEGORY";

    // ==========================================
    // 8. Delivery Mission & OTP Codes
    // ==========================================
    // ── DeliveryMission: creation ────────────────────────────────
    public const string DONATION_ID_REQUIRED = "DONATION_ID_REQUIRED";                            // ✅ NEW
    public const string DROPOFF_LOCATION_REQUIRED = "DROPOFF_LOCATION_REQUIRED";                  // ✅ NEW
    public const string DROPOFF_LOCATION_TOO_LONG = "DROPOFF_LOCATION_TOO_LONG";                  // ✅ NEW

    // ── DeliveryMission: Hangfire ────────────────────────────────
    public const string HANGFIRE_JOB_ID_REQUIRED = "HANGFIRE_JOB_ID_REQUIRED";                    // ✅ NEW

    // ── DeliveryMission: lifecycle state ─────────────────────────
    public const string MISSION_ALREADY_DELIVERED = "MISSION_ALREADY_DELIVERED";                  // ✅ NEW
    public const string MISSION_ALREADY_CANCELLED = "MISSION_ALREADY_CANCELLED";                  // ✅ NEW
    public const string MISSION_CLOSED = "MISSION_CLOSED";                                        // ✅ NEW
    public const string MISSION_DELETED = "MISSION_DELETED";                                      // ✅ NEW
    // MISSION_INVALID_STATUS already defined below
    // MISSION_NOT_IN_TRANSIT already defined below

    // ── DeliveryMission: OTP ─────────────────────────────────────
    public const string OTP_CODE_REQUIRED = "OTP_CODE_REQUIRED";                                  // ✅ NEW
    public const string OTP_NOT_ISSUED = "OTP_NOT_ISSUED";                                        // ✅ NEW
    public const string INVALID_OTP_FORMAT = "INVALID_OTP_FORMAT";                                // ✅ NEW
    public const string OTP_EXPIRY_IN_PAST = "OTP_EXPIRY_IN_PAST";                                // ✅ NEW
    // INVALID_OTP, OTP_EXPIRED, OTP_MAX_ATTEMPTS_EXCEEDED already defined below

    // ── DeliveryMission: existing legacy codes ───────────────────
    public const string MISSION_CREATED = "MISSION_CREATED";
    public const string MISSION_ACCEPTED = "MISSION_ACCEPTED";
    public const string MISSION_PICKUP_CONFIRMED = "MISSION_PICKUP_CONFIRMED";
    public const string MISSION_COMPLETED = "MISSION_COMPLETED";
    public const string MISSION_CANCELLED = "MISSION_CANCELLED";
    public const string MISSION_NOT_FOUND = "MISSION_NOT_FOUND";
    public const string MISSION_ALREADY_ASSIGNED = "MISSION_ALREADY_ASSIGNED";
    public const string MISSION_ALREADY_TAKEN = "MISSION_ALREADY_TAKEN";
    public const string MISSION_INVALID_STATUS = "MISSION_INVALID_STATUS";
    public const string MISSION_NOT_IN_TRANSIT = "MISSION_NOT_IN_TRANSIT";
    public const string MISSION_OTP_RETRIEVED = "MISSION_OTP_RETRIEVED";
    public const string INVALID_OTP = "INVALID_OTP";
    public const string OTP_EXPIRED = "OTP_EXPIRED";
    public const string OTP_MAX_ATTEMPTS_EXCEEDED = "OTP_MAX_ATTEMPTS_EXCEEDED";

    // ==========================================
    // 9. Review & ReviewReason Codes
    // ==========================================
    // ── Review: required fields ──────────────────────────────────
    public const string REVIEW_REVIEWER_ID_REQUIRED = "REVIEW_REVIEWER_ID_REQUIRED";              // ✅ NEW
    public const string REVIEW_REVIEWEE_ID_REQUIRED = "REVIEW_REVIEWEE_ID_REQUIRED";              // ✅ NEW
    public const string REVIEW_MISSION_ID_REQUIRED = "REVIEW_MISSION_ID_REQUIRED";                // ✅ NEW

    // ── Review: value constraints ────────────────────────────────
    public const string INVALID_REVIEW_REASON_ID = "INVALID_REVIEW_REASON_ID";                    // ✅ NEW
    public const string REVIEW_COMMENT_TOO_LONG = "REVIEW_COMMENT_TOO_LONG";                      // ✅ NEW
    public const string REVIEW_DELETED = "REVIEW_DELETED";                                        // ✅ NEW

    // ── Review: existing legacy codes ────────────────────────────
    public const string REVIEW_CREATED = "REVIEW_CREATED";
    public const string REVIEW_NOT_FOUND = "REVIEW_NOT_FOUND";
    public const string REVIEW_ALREADY_EXISTS = "REVIEW_ALREADY_EXISTS";
    public const string REVIEW_INVALID_MISSION = "REVIEW_INVALID_MISSION";
    public const string REVIEW_CANNOT_REVIEW_SELF = "REVIEW_CANNOT_REVIEW_SELF";
    public const string REVIEW_RATING_OUT_OF_RANGE = "REVIEW_RATING_OUT_OF_RANGE";
    public const string REVIEW_REASONS_RETRIEVED = "REVIEW_REASONS_RETRIEVED";

    // ── ReviewReason ─────────────────────────────────────────────
    public const string REVIEW_REASON_CODE_REQUIRED = "REVIEW_REASON_CODE_REQUIRED";
    public const string INVALID_REVIEW_REASON_FROM_ROLE = "INVALID_REVIEW_REASON_FROM_ROLE";
    public const string INVALID_REVIEW_REASON_TO_ROLE = "INVALID_REVIEW_REASON_TO_ROLE";
    public const string REVIEW_REASON_SAME_ROLE_NOT_ALLOWED = "REVIEW_REASON_SAME_ROLE_NOT_ALLOWED";
    public const string REVIEW_REASON_CODE_TOO_LONG = "REVIEW_REASON_CODE_TOO_LONG";
    public const string REVIEW_REASON_CODE_INVALID_FORMAT = "REVIEW_REASON_CODE_INVALID_FORMAT";

    // ==========================================
    // 10. Notification (Domain) Codes
    // ==========================================
    public const string NOTIFICATION_RECIPIENT_ID_REQUIRED = "NOTIFICATION_RECIPIENT_ID_REQUIRED"; // ✅ NEW
    public const string NOTIFICATION_TITLE_REQUIRED = "NOTIFICATION_TITLE_REQUIRED";              // ✅ NEW
    public const string NOTIFICATION_BODY_REQUIRED = "NOTIFICATION_BODY_REQUIRED";                // ✅ NEW
    public const string NOTIFICATION_TITLE_TOO_LONG = "NOTIFICATION_TITLE_TOO_LONG";              // ✅ NEW
    public const string NOTIFICATION_BODY_TOO_LONG = "NOTIFICATION_BODY_TOO_LONG";                // ✅ NEW
    public const string INVALID_NOTIFICATION_TYPE = "INVALID_NOTIFICATION_TYPE";                  // ✅ NEW
    public const string INVALID_RELATED_ENTITY_ID = "INVALID_RELATED_ENTITY_ID";                  // ✅ NEW

    // ── Notification: existing legacy codes ──────────────────────
    public const string NOTIFICATION_CREATED = "NOTIFICATION_CREATED";
    public const string NOTIFICATION_NOT_FOUND = "NOTIFICATION_NOT_FOUND";
    public const string NOTIFICATION_READ_SUCCESS = "NOTIFICATION_READ_SUCCESS";
    public const string NOTIFICATIONS_RETRIEVED = "NOTIFICATIONS_RETRIEVED";
    public const string NOTIFICATIONS_COUNT_RETRIEVED = "NOTIFICATIONS_COUNT_RETRIEVED";

    // ==========================================
    // 11. PenaltyLog (Domain) Codes
    // ==========================================
    public const string PENALTY_ACCOUNT_ID_REQUIRED = "PENALTY_ACCOUNT_ID_REQUIRED";              // ✅ NEW
    public const string PENALTY_REASON_CODE_REQUIRED = "PENALTY_REASON_CODE_REQUIRED";            // ✅ NEW
    public const string PENALTY_REASON_CODE_TOO_LONG = "PENALTY_REASON_CODE_TOO_LONG";            // ✅ NEW
    public const string INVALID_PENALTY_AMOUNT = "INVALID_PENALTY_AMOUNT";                        // ✅ NEW
    public const string INVALID_MISSION_ID = "INVALID_MISSION_ID";                                // ✅ NEW

    // ==========================================
    // 12. PointTransaction (Domain) Codes
    // ==========================================
    public const string POINT_TRANSACTION_USER_ID_REQUIRED = "POINT_TRANSACTION_USER_ID_REQUIRED"; // ✅ NEW
    public const string INVALID_TRANSACTION_AMOUNT = "INVALID_TRANSACTION_AMOUNT";                // ✅ NEW
    public const string TRANSACTION_AMOUNT_EXCEEDS_LIMIT = "TRANSACTION_AMOUNT_EXCEEDS_LIMIT";    // ✅ NEW
    public const string INVALID_TRANSACTION_TYPE = "INVALID_TRANSACTION_TYPE";                    // ✅ NEW
    public const string INVALID_REFERENCE_ID = "INVALID_REFERENCE_ID";                            // ✅ NEW
    public const string MISSION_COMPLETED_MUST_BE_CREDIT = "MISSION_COMPLETED_MUST_BE_CREDIT";    // ✅ NEW
    public const string PENALTY_MUST_BE_DEBIT = "PENALTY_MUST_BE_DEBIT";                          // ✅ NEW
    public const string REDEMPTION_MUST_BE_DEBIT = "REDEMPTION_MUST_BE_DEBIT";                    // ✅ NEW
    public const string UNSUPPORTED_TRANSACTION_TYPE = "UNSUPPORTED_TRANSACTION_TYPE";            // ✅ NEW

    // ==========================================
    // 13. RefreshToken (Domain) Codes
    // ==========================================
    public const string REFRESH_TOKEN_ACCOUNT_ID_REQUIRED = "REFRESH_TOKEN_ACCOUNT_ID_REQUIRED";  // ✅ NEW
    public const string REFRESH_TOKEN_VALUE_REQUIRED = "REFRESH_TOKEN_VALUE_REQUIRED";            // ✅ NEW
    public const string NEW_TOKEN_REQUIRED = "NEW_TOKEN_REQUIRED";                                // ✅ NEW
    public const string REPLACEMENT_TOKEN_REQUIRED = "REPLACEMENT_TOKEN_REQUIRED";                // ✅ NEW
    public const string REFRESH_TOKEN_TOO_LONG = "REFRESH_TOKEN_TOO_LONG";                        // ✅ NEW
    public const string REPLACEMENT_TOKEN_TOO_LONG = "REPLACEMENT_TOKEN_TOO_LONG";                // ✅ NEW
    public const string REVOCATION_REASON_TOO_LONG = "REVOCATION_REASON_TOO_LONG";                // ✅ NEW
    public const string REFRESH_TOKEN_EXPIRY_IN_PAST = "REFRESH_TOKEN_EXPIRY_IN_PAST";            // ✅ NEW
    public const string REPLACEMENT_TOKEN_MATCHES_CURRENT = "REPLACEMENT_TOKEN_MATCHES_CURRENT";  // ✅ NEW
    public const string REFRESH_TOKEN_ALREADY_REVOKED = "REFRESH_TOKEN_ALREADY_REVOKED";          // ✅ NEW

    // ==========================================
    // 14. Notifications, Points, & Stats
    // ==========================================
    public const string INSUFFICIENT_POINTS = "INSUFFICIENT_POINTS";
    public const string STATISTICS_RETRIEVED = "STATISTICS_RETRIEVED";

    // ==========================================
    // 15. Geographic & Meta Data
    // ==========================================
    public const string COUNTRY_NOT_FOUND = "COUNTRY_NOT_FOUND";
    public const string GOVERNORATE_NOT_FOUND = "GOVERNORATE_NOT_FOUND";
    public const string CITY_NOT_FOUND = "CITY_NOT_FOUND";
    public const string COUNTRIES_RETRIEVED = "COUNTRIES_RETRIEVED";
    public const string GOVERNORATES_RETRIEVED = "GOVERNORATES_RETRIEVED";
    public const string CITIES_RETRIEVED = "CITIES_RETRIEVED";
}

/// <summary>
/// Alias class for AppMessageCodes to ensure seamless compatibility.
/// </summary>
public static class ErrorCodes
{
    // General
    public const string SUCCESS = AppMessageCodes.SUCCESS;
    public const string INTERNAL_SERVER_ERROR = AppMessageCodes.INTERNAL_SERVER_ERROR;
    public const string VALIDATION_ERROR = AppMessageCodes.VALIDATION_ERROR;
    public const string NOT_FOUND = AppMessageCodes.NOT_FOUND;
    public const string UNAUTHORIZED = AppMessageCodes.UNAUTHORIZED;
    public const string FORBIDDEN = AppMessageCodes.FORBIDDEN;
    public const string CONFLICT = AppMessageCodes.CONFLICT;
    public const string CONCURRENCY_CONFLICT = AppMessageCodes.CONCURRENCY_CONFLICT;
    public const string INVALID_INPUT = AppMessageCodes.INVALID_INPUT;
    public const string OPERATION_NOT_ALLOWED = AppMessageCodes.OPERATION_NOT_ALLOWED;
    public const string TOO_MANY_REQUESTS = AppMessageCodes.TOO_MANY_REQUESTS;

    // Files
    public const string FILE_REQUIRED = AppMessageCodes.FILE_REQUIRED;
    public const string FILE_TOO_LARGE = AppMessageCodes.FILE_TOO_LARGE;
    public const string INVALID_FILE_TYPE = AppMessageCodes.INVALID_FILE_TYPE;
    public const string UPLOAD_FAILED = AppMessageCodes.UPLOAD_FAILED;
    public const string UPLOAD_SUCCESS = AppMessageCodes.UPLOAD_SUCCESS;
    public const string UPDATE_SUCCESS = AppMessageCodes.UPDATE_SUCCESS;

    // Auth
    public const string LOGIN_SUCCESS = AppMessageCodes.LOGIN_SUCCESS;
    public const string REGISTER_SUCCESS = AppMessageCodes.REGISTER_SUCCESS;
    public const string NOT_SUPPORT_REGEST_ADMIN = AppMessageCodes.NOT_SUPPORT_REGEST_ADMIN;
    public const string LOGOUT_SUCCESS = AppMessageCodes.LOGOUT_SUCCESS;
    public const string INVALID_CREDENTIALS = AppMessageCodes.INVALID_CREDENTIALS;
    public const string EMAIL_ALREADY_EXISTS = AppMessageCodes.EMAIL_ALREADY_EXISTS;
    public const string PHONE_ALREADY_EXISTS = AppMessageCodes.PHONE_ALREADY_EXISTS;
    public const string USER_NOT_FOUND = AppMessageCodes.USER_NOT_FOUND;
    public const string ACCOUNT_NOT_FOUND = AppMessageCodes.ACCOUNT_NOT_FOUND;
    public const string ACCOUNT_DEACTIVATED = AppMessageCodes.ACCOUNT_DEACTIVATED;
    public const string ACCOUNT_SUSPENDED = AppMessageCodes.ACCOUNT_SUSPENDED;
    public const string INVALID_TOKEN = AppMessageCodes.INVALID_TOKEN;
    public const string TOKEN_EXPIRED = AppMessageCodes.TOKEN_EXPIRED;
    public const string REFRESH_TOKEN_REVOKED = AppMessageCodes.REFRESH_TOKEN_REVOKED;
    public const string PASSWORD_MISMATCH = AppMessageCodes.PASSWORD_MISMATCH;
    public const string PASSWORD_INCORRECT = AppMessageCodes.PASSWORD_INCORRECT;
    public const string PASSWORD_TOO_WEAK = AppMessageCodes.PASSWORD_TOO_WEAK;
    public const string PASSWORD_CHANGE_SUCCESS = AppMessageCodes.PASSWORD_CHANGE_SUCCESS;
    public const string ME_SUCCESS = AppMessageCodes.ME_SUCCESS;
    public const string PROFILE_RETRIEVED = AppMessageCodes.PROFILE_RETRIEVED;
    public const string PROFILE_UPDATED = AppMessageCodes.PROFILE_UPDATED;
    public const string USER_STATUS_UPDATED = AppMessageCodes.USER_STATUS_UPDATED;
    public const string USERS_RETRIEVED = AppMessageCodes.USERS_RETRIEVED;
    public const string PASSWORD_RESET_EMAIL_SENT = AppMessageCodes.PASSWORD_RESET_EMAIL_SENT;
    public const string PASSWORD_RESET_SUCCESS = AppMessageCodes.PASSWORD_RESET_SUCCESS;
    public const string EMAIL_VERIFIED_SUCCESS = AppMessageCodes.EMAIL_VERIFIED_SUCCESS;
    public const string PUBLIC_PROFILE_RETRIEVED = AppMessageCodes.PUBLIC_PROFILE_RETRIEVED;
    public const string CATEGORIES_RETRIEVED = AppMessageCodes.CATEGORIES_RETRIEVED;
    public const string POINTS_RETRIEVED = AppMessageCodes.POINTS_RETRIEVED;
    public const string LEADERBOARD_RETRIEVED = AppMessageCodes.LEADERBOARD_RETRIEVED;
    public const string DASHBOARD_RETRIEVED = AppMessageCodes.DASHBOARD_RETRIEVED;
    public const string INVALID_ROLE = AppMessageCodes.INVALID_ROLE;
    public const string INVALID_USER_TYPE = AppMessageCodes.INVALID_USER_TYPE;

    // Roles
    public const string DONOR_NOT_FOUND = AppMessageCodes.DONOR_NOT_FOUND;
    public const string CHARITY_NOT_FOUND = AppMessageCodes.CHARITY_NOT_FOUND;
    public const string CHARITY_NOT_VERIFIED = AppMessageCodes.CHARITY_NOT_VERIFIED;
    public const string VOLUNTEER_NOT_FOUND = AppMessageCodes.VOLUNTEER_NOT_FOUND;
    public const string VOLUNTEER_INACTIVE = AppMessageCodes.VOLUNTEER_INACTIVE;
    public const string VOLUNTEER_FETCHED = AppMessageCodes.VOLUNTEER_FETCHED;
    public const string VOLUNTEER_CREATED = AppMessageCodes.VOLUNTEER_CREATED;
    public const string VOLUNTEER_UPDATED = AppMessageCodes.VOLUNTEER_UPDATED;
    public const string VOLUNTEER_DELETED = AppMessageCodes.VOLUNTEER_DELETED;
    public const string VOLUNTEER_ASSIGNED = AppMessageCodes.VOLUNTEER_ASSIGNED;
    public const string VOLUNTEER_AVAILABILITY_UPDATED = AppMessageCodes.VOLUNTEER_AVAILABILITY_UPDATED;

    // Account (Domain)
    public const string FULL_NAME_REQUIRED = AppMessageCodes.FULL_NAME_REQUIRED;
    public const string EMAIL_REQUIRED = AppMessageCodes.EMAIL_REQUIRED;
    public const string PHONE_REQUIRED = AppMessageCodes.PHONE_REQUIRED;
    public const string PASSWORD_HASH_REQUIRED = AppMessageCodes.PASSWORD_HASH_REQUIRED;
    public const string IMAGE_PATH_REQUIRED = AppMessageCodes.IMAGE_PATH_REQUIRED;
    public const string INVALID_CITY_ID = AppMessageCodes.INVALID_CITY_ID;
    public const string INVALID_POINTS_AMOUNT = AppMessageCodes.INVALID_POINTS_AMOUNT;
    public const string INVALID_TRUST_SCORE_AMOUNT = AppMessageCodes.INVALID_TRUST_SCORE_AMOUNT;
    public const string ADMIN_CANNOT_BE_COMPANY = AppMessageCodes.ADMIN_CANNOT_BE_COMPANY;
    public const string CHARITY_MUST_BE_COMPANY = AppMessageCodes.CHARITY_MUST_BE_COMPANY;
    public const string WALLET_POINTS_LIMIT_EXCEEDED = AppMessageCodes.WALLET_POINTS_LIMIT_EXCEEDED;
    public const string ACCOUNT_DELETED = AppMessageCodes.ACCOUNT_DELETED;
    public const string CHARITY_ONLY_OPERATION = AppMessageCodes.CHARITY_ONLY_OPERATION;
    public const string VOLUNTEER_ONLY_OPERATION = AppMessageCodes.VOLUNTEER_ONLY_OPERATION;

    // Category
    public const string CATEGORY_NAME_AR_REQUIRED = AppMessageCodes.CATEGORY_NAME_AR_REQUIRED;
    public const string CATEGORY_NAME_EN_REQUIRED = AppMessageCodes.CATEGORY_NAME_EN_REQUIRED;
    public const string CATEGORY_NAME_AR_TOO_LONG = AppMessageCodes.CATEGORY_NAME_AR_TOO_LONG;
    public const string CATEGORY_NAME_EN_TOO_LONG = AppMessageCodes.CATEGORY_NAME_EN_TOO_LONG;

    // Donation
    public const string DONOR_ID_REQUIRED = AppMessageCodes.DONOR_ID_REQUIRED;
    public const string CHARITY_ID_REQUIRED = AppMessageCodes.CHARITY_ID_REQUIRED;
    public const string VOLUNTEER_ID_REQUIRED = AppMessageCodes.VOLUNTEER_ID_REQUIRED;
    public const string FOOD_TYPE_REQUIRED = AppMessageCodes.FOOD_TYPE_REQUIRED;
    public const string QUANTITY_REQUIRED = AppMessageCodes.QUANTITY_REQUIRED;
    public const string PICKUP_LOCATION_REQUIRED = AppMessageCodes.PICKUP_LOCATION_REQUIRED;
    public const string CANCELLATION_REASON_REQUIRED = AppMessageCodes.CANCELLATION_REASON_REQUIRED;
    public const string FOOD_TYPE_TOO_LONG = AppMessageCodes.FOOD_TYPE_TOO_LONG;
    public const string QUANTITY_TOO_LONG = AppMessageCodes.QUANTITY_TOO_LONG;
    public const string PICKUP_LOCATION_TOO_LONG = AppMessageCodes.PICKUP_LOCATION_TOO_LONG;
    public const string TITLE_TOO_LONG = AppMessageCodes.TITLE_TOO_LONG;
    public const string DESCRIPTION_TOO_LONG = AppMessageCodes.DESCRIPTION_TOO_LONG;
    public const string CANCELLATION_REASON_TOO_LONG = AppMessageCodes.CANCELLATION_REASON_TOO_LONG;
}