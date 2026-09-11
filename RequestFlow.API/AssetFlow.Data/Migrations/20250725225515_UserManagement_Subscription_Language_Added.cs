using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserManagement_Subscription_Language_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Verb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsBackEnd = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_ApplicationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    ResourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleResources_ApplicationRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserRoles_ApplicationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstLetter = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsReset = table.Column<bool>(type: "bit", nullable: false),
                    IsPlatformUser = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastActive = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationUsers_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Languages_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Languages_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MenuItemEnums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemEnums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemEnums_ApplicationRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItemEnums_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MenuItemEnums_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MetaDataKeysEnums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaDataKeysEnums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaDataKeysEnums_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MetaDataKeysEnums_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NavigationCreateItemEnums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationCreateItemEnums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavigationCreateItemEnums_ApplicationRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NavigationCreateItemEnums_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NavigationCreateItemEnums_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NavigationItemEnums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationItemEnums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavigationItemEnums_ApplicationRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NavigationItemEnums_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NavigationItemEnums_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Order = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionTypes_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubscriptionTypes_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SubscriptionTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_ApplicationUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tenants_ApplicationUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tenants_SubscriptionTypes_SubscriptionTypeId",
                        column: x => x.SubscriptionTypeId,
                        principalTable: "SubscriptionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantLanguages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantLanguages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_CreatedById",
                table: "ApplicationRoles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_ModifiedById",
                table: "ApplicationRoles",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_TenantId",
                table: "ApplicationRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "ApplicationRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserRoles_RoleId",
                table: "ApplicationUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "ApplicationUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_CreatedById",
                table: "ApplicationUsers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_ModifiedById",
                table: "ApplicationUsers",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_TenantId",
                table: "ApplicationUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "ApplicationUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_CreatedById",
                table: "Languages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Languages_ModifiedById",
                table: "Languages",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemEnums_ApplicationRoleId",
                table: "MenuItemEnums",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemEnums_CreatedById",
                table: "MenuItemEnums",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemEnums_ModifiedById",
                table: "MenuItemEnums",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_MetaDataKeysEnums_CreatedById",
                table: "MetaDataKeysEnums",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MetaDataKeysEnums_ModifiedById",
                table: "MetaDataKeysEnums",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationCreateItemEnums_ApplicationRoleId",
                table: "NavigationCreateItemEnums",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationCreateItemEnums_CreatedById",
                table: "NavigationCreateItemEnums",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationCreateItemEnums_ModifiedById",
                table: "NavigationCreateItemEnums",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItemEnums_ApplicationRoleId",
                table: "NavigationItemEnums",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItemEnums_CreatedById",
                table: "NavigationItemEnums",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItemEnums_ModifiedById",
                table: "NavigationItemEnums",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoleResources_ApplicationRoleId",
                table: "RoleResources",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleResources_ResourceId",
                table: "RoleResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTypes_CreatedById",
                table: "SubscriptionTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTypes_ModifiedById",
                table: "SubscriptionTypes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TenantLanguages_LanguageId",
                table: "TenantLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantLanguages_TenantId",
                table: "TenantLanguages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CreatedById",
                table: "Tenants",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ModifiedById",
                table: "Tenants",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_SubscriptionTypeId",
                table: "Tenants",
                column: "SubscriptionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_ApplicationUsers_CreatedById",
                table: "ApplicationRoles",
                column: "CreatedById",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_ApplicationUsers_ModifiedById",
                table: "ApplicationRoles",
                column: "ModifiedById",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRoles_Tenants_TenantId",
                table: "ApplicationRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserRoles_ApplicationUsers_UserId",
                table: "ApplicationUserRoles",
                column: "UserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Tenants_TenantId",
                table: "ApplicationUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionTypes_ApplicationUsers_CreatedById",
                table: "SubscriptionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionTypes_ApplicationUsers_ModifiedById",
                table: "SubscriptionTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_ApplicationUsers_CreatedById",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_ApplicationUsers_ModifiedById",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "ApplicationUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "MenuItemEnums");

            migrationBuilder.DropTable(
                name: "MetaDataKeysEnums");

            migrationBuilder.DropTable(
                name: "NavigationCreateItemEnums");

            migrationBuilder.DropTable(
                name: "NavigationItemEnums");

            migrationBuilder.DropTable(
                name: "RoleResources");

            migrationBuilder.DropTable(
                name: "TenantLanguages");

            migrationBuilder.DropTable(
                name: "ApplicationRoles");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "SubscriptionTypes");
        }
    }
}
