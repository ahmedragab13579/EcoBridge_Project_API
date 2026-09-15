namespace EcoBridge.Domain.Errors;

public static class AppMessageCodes
{
    // ==========================================
    // 1. General & System Codes
    // ==========================================
    public const string SUCCESS = "SUCCESS";
    public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
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
    // 5. Donations
    // ==========================================
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
    // 6. Donation Requests
    // ==========================================
    public const string DONATION_REQUEST_CREATED = "DONATION_REQUEST_CREATED";
    public const string DONATION_REQUEST_NOT_FOUND = "DONATION_REQUEST_NOT_FOUND";
    public const string DONATION_REQUEST_ALREADY_EXISTS = "DONATION_REQUEST_ALREADY_EXISTS";
    public const string DONATION_REQUEST_ALREADY_APPROVED = "DONATION_REQUEST_ALREADY_APPROVED";
    public const string DONATION_REQUEST_ALREADY_REJECTED = "DONATION_REQUEST_ALREADY_REJECTED";
    public const string DONATION_REQUEST_APPROVED = "DONATION_REQUEST_APPROVED";
    public const string DONATION_REQUEST_REJECTED = "DONATION_REQUEST_REJECTED";
    public const string DONATION_REQUEST_RETRIEVED = "DONATION_REQUEST_RETRIEVED";

    // ==========================================
    // 7. Delivery Missions & OTP
    // ==========================================
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
    // 8. Reviews
    // ==========================================
    public const string REVIEW_CREATED = "REVIEW_CREATED";
    public const string REVIEW_NOT_FOUND = "REVIEW_NOT_FOUND";
    public const string REVIEW_ALREADY_EXISTS = "REVIEW_ALREADY_EXISTS";
    public const string REVIEW_INVALID_MISSION = "REVIEW_INVALID_MISSION";
    public const string REVIEW_CANNOT_REVIEW_SELF = "REVIEW_CANNOT_REVIEW_SELF";
    public const string REVIEW_RATING_OUT_OF_RANGE = "REVIEW_RATING_OUT_OF_RANGE";
    public const string REVIEW_REASONS_RETRIEVED = "REVIEW_REASONS_RETRIEVED";

    // ==========================================
    // 9. Notifications, Points, & Stats
    // ==========================================
    public const string NOTIFICATION_CREATED = "NOTIFICATION_CREATED";
    public const string NOTIFICATION_NOT_FOUND = "NOTIFICATION_NOT_FOUND";
    public const string NOTIFICATION_READ_SUCCESS = "NOTIFICATION_READ_SUCCESS";
    public const string NOTIFICATIONS_RETRIEVED = "NOTIFICATIONS_RETRIEVED";
    public const string NOTIFICATIONS_COUNT_RETRIEVED = "NOTIFICATIONS_COUNT_RETRIEVED";
    public const string INSUFFICIENT_POINTS = "INSUFFICIENT_POINTS";
    public const string STATISTICS_RETRIEVED = "STATISTICS_RETRIEVED";

    // ==========================================
    // 10. Geographic & Meta Data
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

    public const string FILE_REQUIRED = AppMessageCodes.FILE_REQUIRED;
    public const string FILE_TOO_LARGE = AppMessageCodes.FILE_TOO_LARGE;
    public const string INVALID_FILE_TYPE = AppMessageCodes.INVALID_FILE_TYPE;
    public const string UPLOAD_FAILED = AppMessageCodes.UPLOAD_FAILED;
    public const string UPLOAD_SUCCESS = AppMessageCodes.UPLOAD_SUCCESS;
    public const string UPDATE_SUCCESS = AppMessageCodes.UPDATE_SUCCESS;

    public const string LOGIN_SUCCESS = AppMessageCodes.LOGIN_SUCCESS;
    public const string REGISTER_SUCCESS = AppMessageCodes.REGISTER_SUCCESS;
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

    public const string DONATION_CREATED = AppMessageCodes.DONATION_CREATED;
    public const string DONATION_UPDATED = AppMessageCodes.DONATION_UPDATED;
    public const string DONATION_DELETED = AppMessageCodes.DONATION_DELETED;
    public const string DONATION_RETRIEVED = AppMessageCodes.DONATION_RETRIEVED;
    public const string DONATIONS_RETRIEVED = AppMessageCodes.DONATIONS_RETRIEVED;
    public const string DONATION_NOT_FOUND = AppMessageCodes.DONATION_NOT_FOUND;
    public const string DONATION_EXPIRED = AppMessageCodes.DONATION_EXPIRED;
    public const string DONATION_ALREADY_REQUESTED = AppMessageCodes.DONATION_ALREADY_REQUESTED;
    public const string DONATION_ALREADY_ACCEPTED = AppMessageCodes.DONATION_ALREADY_ACCEPTED;
    public const string DONATION_ALREADY_COMPLETED = AppMessageCodes.DONATION_ALREADY_COMPLETED;
    public const string DONATION_ALREADY_CANCELLED = AppMessageCodes.DONATION_ALREADY_CANCELLED;
    public const string DONATION_STATUS_UPDATED = AppMessageCodes.DONATION_STATUS_UPDATED;
    public const string DONATION_CANCELLED = AppMessageCodes.DONATION_CANCELLED;
    public const string DONATION_ACCEPTED = AppMessageCodes.DONATION_ACCEPTED;
    public const string DONATION_CANNOT_BE_MODIFIED = AppMessageCodes.DONATION_CANNOT_BE_MODIFIED;
    public const string INVALID_DONATION_QUANTITY = AppMessageCodes.INVALID_DONATION_QUANTITY;
    public const string INVALID_DONATION_CATEGORY = AppMessageCodes.INVALID_DONATION_CATEGORY;

    public const string DONATION_REQUEST_CREATED = AppMessageCodes.DONATION_REQUEST_CREATED;
    public const string DONATION_REQUEST_NOT_FOUND = AppMessageCodes.DONATION_REQUEST_NOT_FOUND;
    public const string DONATION_REQUEST_ALREADY_EXISTS = AppMessageCodes.DONATION_REQUEST_ALREADY_EXISTS;
    public const string DONATION_REQUEST_ALREADY_APPROVED = AppMessageCodes.DONATION_REQUEST_ALREADY_APPROVED;
    public const string DONATION_REQUEST_ALREADY_REJECTED = AppMessageCodes.DONATION_REQUEST_ALREADY_REJECTED;
    public const string DONATION_REQUEST_APPROVED = AppMessageCodes.DONATION_REQUEST_APPROVED;
    public const string DONATION_REQUEST_REJECTED = AppMessageCodes.DONATION_REQUEST_REJECTED;
    public const string DONATION_REQUEST_RETRIEVED = AppMessageCodes.DONATION_REQUEST_RETRIEVED;

    public const string MISSION_CREATED = AppMessageCodes.MISSION_CREATED;
    public const string MISSION_ACCEPTED = AppMessageCodes.MISSION_ACCEPTED;
    public const string MISSION_PICKUP_CONFIRMED = AppMessageCodes.MISSION_PICKUP_CONFIRMED;
    public const string MISSION_COMPLETED = AppMessageCodes.MISSION_COMPLETED;
    public const string MISSION_CANCELLED = AppMessageCodes.MISSION_CANCELLED;
    public const string MISSION_NOT_FOUND = AppMessageCodes.MISSION_NOT_FOUND;
    public const string MISSION_ALREADY_ASSIGNED = AppMessageCodes.MISSION_ALREADY_ASSIGNED;
    public const string MISSION_ALREADY_TAKEN = AppMessageCodes.MISSION_ALREADY_TAKEN;
    public const string MISSION_INVALID_STATUS = AppMessageCodes.MISSION_INVALID_STATUS;
    public const string MISSION_NOT_IN_TRANSIT = AppMessageCodes.MISSION_NOT_IN_TRANSIT;
    public const string MISSION_OTP_RETRIEVED = AppMessageCodes.MISSION_OTP_RETRIEVED;
    public const string INVALID_OTP = AppMessageCodes.INVALID_OTP;
    public const string OTP_EXPIRED = AppMessageCodes.OTP_EXPIRED;
    public const string OTP_MAX_ATTEMPTS_EXCEEDED = AppMessageCodes.OTP_MAX_ATTEMPTS_EXCEEDED;

    public const string REVIEW_CREATED = AppMessageCodes.REVIEW_CREATED;
    public const string REVIEW_NOT_FOUND = AppMessageCodes.REVIEW_NOT_FOUND;
    public const string REVIEW_ALREADY_EXISTS = AppMessageCodes.REVIEW_ALREADY_EXISTS;
    public const string REVIEW_INVALID_MISSION = AppMessageCodes.REVIEW_INVALID_MISSION;
    public const string REVIEW_CANNOT_REVIEW_SELF = AppMessageCodes.REVIEW_CANNOT_REVIEW_SELF;
    public const string REVIEW_RATING_OUT_OF_RANGE = AppMessageCodes.REVIEW_RATING_OUT_OF_RANGE;
    public const string REVIEW_REASONS_RETRIEVED = AppMessageCodes.REVIEW_REASONS_RETRIEVED;

    public const string NOTIFICATION_CREATED = AppMessageCodes.NOTIFICATION_CREATED;
    public const string NOTIFICATION_NOT_FOUND = AppMessageCodes.NOTIFICATION_NOT_FOUND;
    public const string NOTIFICATION_READ_SUCCESS = AppMessageCodes.NOTIFICATION_READ_SUCCESS;
    public const string NOTIFICATIONS_RETRIEVED = AppMessageCodes.NOTIFICATIONS_RETRIEVED;
    public const string NOTIFICATIONS_COUNT_RETRIEVED = AppMessageCodes.NOTIFICATIONS_COUNT_RETRIEVED;
    public const string INSUFFICIENT_POINTS = AppMessageCodes.INSUFFICIENT_POINTS;
    public const string STATISTICS_RETRIEVED = AppMessageCodes.STATISTICS_RETRIEVED;

    public const string COUNTRY_NOT_FOUND = AppMessageCodes.COUNTRY_NOT_FOUND;
    public const string GOVERNORATE_NOT_FOUND = AppMessageCodes.GOVERNORATE_NOT_FOUND;
    public const string CITY_NOT_FOUND = AppMessageCodes.CITY_NOT_FOUND;
    public const string COUNTRIES_RETRIEVED = AppMessageCodes.COUNTRIES_RETRIEVED;
    public const string GOVERNORATES_RETRIEVED = AppMessageCodes.GOVERNORATES_RETRIEVED;
    public const string CITIES_RETRIEVED = AppMessageCodes.CITIES_RETRIEVED;
}
