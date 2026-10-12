using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoBridge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_NameAr_NotEmpty", "LEN(LTRIM(RTRIM([NameAr]))) > 0");
                    table.CheckConstraint("CK_Categories_NameEn_NotEmpty", "LEN(LTRIM(RTRIM([NameEn]))) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                    table.CheckConstraint("CK_Countries_Code_NotEmpty", "LEN(LTRIM(RTRIM([Code]))) > 0");
                    table.CheckConstraint("CK_Countries_NameAr_NotEmpty", "LEN(LTRIM(RTRIM([NameAr]))) > 0");
                    table.CheckConstraint("CK_Countries_NameEn_NotEmpty", "LEN(LTRIM(RTRIM([NameEn]))) > 0");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FromRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPositive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReasons", x => x.Id);
                    table.CheckConstraint("CK_ReviewReasons_Code_NotEmpty", "LEN(LTRIM(RTRIM([Code]))) > 0");
                    table.CheckConstraint("CK_ReviewReasons_FromRole_Not_ToRole", "[FromRole] <> [ToRole]");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                    table.CheckConstraint("CK_Governorates_Code_NotEmpty", "LEN(LTRIM(RTRIM([Code]))) > 0");
                    table.CheckConstraint("CK_Governorates_NameAr_NotEmpty", "LEN(LTRIM(RTRIM([NameAr]))) > 0");
                    table.CheckConstraint("CK_Governorates_NameEn_NotEmpty", "LEN(LTRIM(RTRIM([NameEn]))) > 0");
                    table.ForeignKey(
                        name: "FK_Governorates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.CheckConstraint("CK_Cities_Code_NotEmpty", "LEN(LTRIM(RTRIM([Code]))) > 0");
                    table.CheckConstraint("CK_Cities_NameAr_NotEmpty", "LEN(LTRIM(RTRIM([NameAr]))) > 0");
                    table.CheckConstraint("CK_Cities_NameEn_NotEmpty", "LEN(LTRIM(RTRIM([NameEn]))) > 0");
                    table.ForeignKey(
                        name: "FK_Cities_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalWalletPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TrustScore = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    AddressDetails = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrganizationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VehicleDetails = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.CheckConstraint("CK_Accounts_TrustScore_Range", "[TrustScore] >= 0 AND [TrustScore] <= 100");
                    table.CheckConstraint("CK_Accounts_WalletPoints_Range", "[TotalWalletPoints] >= 0 AND [TotalWalletPoints] <= 1000000");
                    table.ForeignKey(
                        name: "FK_Accounts_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VolunteerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FoodType = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuantityDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.CheckConstraint("CK_Donations_ExpiryDate_Future", "[ExpiryDate] >= [CreatedAt]");
                    table.CheckConstraint("CK_Donations_FoodType_NotEmpty", "LEN(LTRIM(RTRIM([FoodType]))) > 0");
                    table.CheckConstraint("CK_Donations_PickupLocation_NotEmpty", "LEN(LTRIM(RTRIM([PickupLocation]))) > 0");
                    table.CheckConstraint("CK_Donations_Quantity_NotEmpty", "LEN(LTRIM(RTRIM([QuantityDescription]))) > 0");
                    table.CheckConstraint("CK_Donations_Weight_Positive", "[WeightKg] IS NULL OR ([WeightKg] > 0 AND [WeightKg] <= 100000)");
                    table.ForeignKey(
                        name: "FK_Donations_Accounts_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_Accounts_DonorId",
                        column: x => x.DonorId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_Accounts_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Donations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.CheckConstraint("CK_Notifications_Body_NotEmpty", "LEN(LTRIM(RTRIM([Body]))) > 0");
                    table.CheckConstraint("CK_Notifications_ReadAt_Consistency", "([IsRead] = 0 AND [ReadAtUtc] IS NULL) OR ([IsRead] = 1 AND [ReadAtUtc] IS NOT NULL)");
                    table.CheckConstraint("CK_Notifications_Title_NotEmpty", "LEN(LTRIM(RTRIM([Title]))) > 0");
                    table.ForeignKey(
                        name: "FK_Notifications_Accounts_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.CheckConstraint("CK_PointTransactions_Amount_Absolute_Limit", "ABS([Amount]) <= 1000000");
                    table.CheckConstraint("CK_PointTransactions_Amount_NonZero", "[Amount] <> 0");
                    table.CheckConstraint("CK_PointTransactions_Sign_Per_Type", "([TransactionType] = 'MissionCompleted' AND [Amount] > 0) OR ([TransactionType] = 'PointsRedeemed' AND [Amount] < 0) OR ([TransactionType] = 'Penalty' AND [Amount] < 0) OR ([TransactionType] = 'UnassignCompensation' AND [Amount] > 0)");
                    table.ForeignKey(
                        name: "FK_PointTransactions_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.CheckConstraint("CK_RefreshTokens_Expiry_After_Creation", "[ExpiresAtUtc] > [CreatedAtUtc]");
                    table.CheckConstraint("CK_RefreshTokens_Token_NotEmpty", "LEN(LTRIM(RTRIM([Token]))) > 0");
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryMissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VolunteerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DropOffLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PickupTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OtpCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    OtpExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OtpAttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HangfireJobId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryMissions", x => x.Id);
                    table.CheckConstraint("CK_DeliveryMissions_DropOffLocation_NotEmpty", "LEN(LTRIM(RTRIM([DropOffLocation]))) > 0");
                    table.CheckConstraint("CK_DeliveryMissions_OtpAttemptCount_Range", "[OtpAttemptCount] >= 0 AND [OtpAttemptCount] <= 5");
                    table.CheckConstraint("CK_DeliveryMissions_PickupLocation_NotEmpty", "LEN(LTRIM(RTRIM([PickupLocation]))) > 0");
                    table.ForeignKey(
                        name: "FK_DeliveryMissions_Accounts_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryMissions_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PenaltyLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PenaltyLogs", x => x.Id);
                    table.CheckConstraint("CK_PenaltyLogs_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_PenaltyLogs_ReasonCode_NotEmpty", "LEN(LTRIM(RTRIM([ReasonCode]))) > 0");
                    table.ForeignKey(
                        name: "FK_PenaltyLogs_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PenaltyLogs_DeliveryMissions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "DeliveryMissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevieweeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewReasonId = table.Column<int>(type: "int", nullable: false),
                    MissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_Comment_NotEmpty_When_Present", "[Comment] IS NULL OR LEN(LTRIM(RTRIM([Comment]))) > 0");
                    table.CheckConstraint("CK_Reviews_Reviewer_Not_Reviewee", "[ReviewerId] <> [RevieweeId]");
                    table.ForeignKey(
                        name: "FK_Reviews_Accounts_RevieweeId",
                        column: x => x.RevieweeId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_Accounts_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_DeliveryMissions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "DeliveryMissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_ReviewReasons_ReviewReasonId",
                        column: x => x.ReviewReasonId,
                        principalTable: "ReviewReasons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrustCreditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustCreditLogs", x => x.Id);
                    table.CheckConstraint("CK_TrustCreditLogs_Amount_MaxLimit", "[Amount] <= 1000000");
                    table.CheckConstraint("CK_TrustCreditLogs_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_TrustCreditLogs_ReasonCode_NotEmpty", "LEN(LTRIM(RTRIM([ReasonCode]))) > 0");
                    table.ForeignKey(
                        name: "FK_TrustCreditLogs_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrustCreditLogs_Accounts_AccountId1",
                        column: x => x.AccountId1,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrustCreditLogs_DeliveryMissions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "DeliveryMissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Accounts",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CityId",
                table: "Accounts",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email_IsDeleted",
                table: "Accounts",
                columns: new[] { "Email", "IsDeleted" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsActive_IsDeleted",
                table: "Accounts",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NormalizedEmail_IsDeleted",
                table: "Accounts",
                columns: new[] { "NormalizedEmail", "IsDeleted" },
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_NormalizedUserName_IsDeleted",
                table: "Accounts",
                columns: new[] { "NormalizedUserName", "IsDeleted" },
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PhoneNumber",
                table: "Accounts",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Role",
                table: "Accounts",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Role_Available_City",
                table: "Accounts",
                columns: new[] { "Role", "IsAvailable", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserName_IsDeleted",
                table: "Accounts",
                columns: new[] { "UserName", "IsDeleted" },
                unique: true,
                filter: "[UserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Accounts",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_NameAr",
                table: "Categories",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Categories_NameEn",
                table: "Categories",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_GovernorateId",
                table: "Cities",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "UX_Cities_Code",
                table: "Cities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Cities_GovernorateId_NameAr",
                table: "Cities",
                columns: new[] { "GovernorateId", "NameAr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Cities_GovernorateId_NameEn",
                table: "Cities",
                columns: new[] { "GovernorateId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Countries_NameAr",
                table: "Countries",
                column: "NameAr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Countries_NameEn",
                table: "Countries",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMissions_CreatedAt",
                table: "DeliveryMissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMissions_DonationId_DeliveryTime",
                table: "DeliveryMissions",
                columns: new[] { "DonationId", "DeliveryTime" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMissions_OtpExpiresAtUtc",
                table: "DeliveryMissions",
                column: "OtpExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMissions_Status",
                table: "DeliveryMissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMissions_VolunteerId",
                table: "DeliveryMissions",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryMissions_VolunteerId_DeliveryTime",
                table: "DeliveryMissions",
                columns: new[] { "VolunteerId", "DeliveryTime" });

            migrationBuilder.CreateIndex(
                name: "UX_DeliveryMissions_DonationId_Active",
                table: "DeliveryMissions",
                column: "DonationId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CategoryId",
                table: "Donations",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CharityId_Status",
                table: "Donations",
                columns: new[] { "CharityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CityId",
                table: "Donations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CreatedAt",
                table: "Donations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonorId",
                table: "Donations",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_ExpiryDate",
                table: "Donations",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_Status_CityId",
                table: "Donations",
                columns: new[] { "Status", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_Status_ExpiryDate",
                table: "Donations",
                columns: new[] { "Status", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_VolunteerId_Status",
                table: "Donations",
                columns: new[] { "VolunteerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_CountryId",
                table: "Governorates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "UX_Governorates_Code",
                table: "Governorates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Governorates_CountryId_NameAr",
                table: "Governorates",
                columns: new[] { "CountryId", "NameAr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Governorates_CountryId_NameEn",
                table: "Governorates",
                columns: new[] { "CountryId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAtUtc",
                table: "Notifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId_IsRead_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedEntityId",
                table: "Notifications",
                column: "RelatedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredOnUtc",
                table: "OutboxMessages",
                column: "OccurredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_NextRetryOnUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "NextRetryOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyLogs_AccountId",
                table: "PenaltyLogs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyLogs_AccountId_CreatedAtUtc",
                table: "PenaltyLogs",
                columns: new[] { "AccountId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyLogs_CreatedAtUtc",
                table: "PenaltyLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyLogs_MissionId",
                table: "PenaltyLogs",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PenaltyLogs_ReasonCode",
                table: "PenaltyLogs",
                column: "ReasonCode");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_CreatedAt",
                table: "PointTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_ReferenceId",
                table: "PointTransactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_TransactionType",
                table: "PointTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId",
                table: "PointTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId_CreatedAt",
                table: "PointTransactions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AccountId",
                table: "RefreshTokens",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AccountId_RevokedAtUtc",
                table: "RefreshTokens",
                columns: new[] { "AccountId", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAtUtc",
                table: "RefreshTokens",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "UX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReasons_FromRole_ToRole_IsPositive",
                table: "ReviewReasons",
                columns: new[] { "FromRole", "ToRole", "IsPositive" });

            migrationBuilder.CreateIndex(
                name: "UX_ReviewReasons_Code",
                table: "ReviewReasons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CreatedAt",
                table: "Reviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MissionId",
                table: "Reviews",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RevieweeId",
                table: "Reviews",
                column: "RevieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RevieweeId_CreatedAt",
                table: "Reviews",
                columns: new[] { "RevieweeId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewReasonId",
                table: "Reviews",
                column: "ReviewReasonId");

            migrationBuilder.CreateIndex(
                name: "UX_Reviews_ReviewerId_MissionId",
                table: "Reviews",
                columns: new[] { "ReviewerId", "MissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_AccountId",
                table: "TrustCreditLogs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_AccountId_CreatedAtUtc",
                table: "TrustCreditLogs",
                columns: new[] { "AccountId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_AccountId1",
                table: "TrustCreditLogs",
                column: "AccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_CreatedAtUtc",
                table: "TrustCreditLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_MissionId",
                table: "TrustCreditLogs",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_ReasonCode",
                table: "TrustCreditLogs",
                column: "ReasonCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "PenaltyLogs");

            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "TrustCreditLogs");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ReviewReasons");

            migrationBuilder.DropTable(
                name: "DeliveryMissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
