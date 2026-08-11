using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateDbContextAndAddUserLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKeys_Users_UserId",
                table: "ApiKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictTokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictScopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictAuthorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthClients",
                table: "OAuthClients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiKeys",
                table: "ApiKeys");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "user_roles");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "OAuthClients",
                newName: "o_auth_clients");

            migrationBuilder.RenameTable(
                name: "ApiKeys",
                newName: "api_keys");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "users",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LastLoginAt",
                table: "users",
                newName: "last_login_at");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "users",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "users",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "GoogleId",
                table: "users",
                newName: "google_id");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "users",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "users",
                newName: "email_confirmed");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "users",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "users",
                newName: "avatar_url");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "users",
                newName: "ix_users_username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Id",
                table: "users",
                newName: "ix_users_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GoogleId",
                table: "users",
                newName: "ix_users_google_id");

            migrationBuilder.RenameColumn(
                name: "Permissions",
                table: "roles",
                newName: "permissions");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "roles",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "roles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "roles",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "roles",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "roles",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "roles",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "roles",
                newName: "create_by");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Name",
                table: "roles",
                newName: "ix_roles_name");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Id",
                table: "roles",
                newName: "ix_roles_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictTokens",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictTokens",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OpenIddictTokens",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictTokens",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "OpenIddictTokens",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictTokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "OpenIddictTokens",
                newName: "reference_id");

            migrationBuilder.RenameColumn(
                name: "RedemptionDate",
                table: "OpenIddictTokens",
                newName: "redemption_date");

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "OpenIddictTokens",
                newName: "expiration_date");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OpenIddictTokens",
                newName: "creation_date");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictTokens",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "AuthorizationId",
                table: "OpenIddictTokens",
                newName: "authorization_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "OpenIddictTokens",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_reference_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_authorization_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_application_id_status_subject_type");

            migrationBuilder.RenameColumn(
                name: "Resources",
                table: "OpenIddictScopes",
                newName: "resources");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictScopes",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OpenIddictScopes",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Descriptions",
                table: "OpenIddictScopes",
                newName: "descriptions");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "OpenIddictScopes",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictScopes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DisplayNames",
                table: "OpenIddictScopes",
                newName: "display_names");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "OpenIddictScopes",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictScopes",
                newName: "concurrency_token");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictScopes_Name",
                table: "OpenIddictScopes",
                newName: "ix_open_iddict_scopes_name");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictAuthorizations",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictAuthorizations",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OpenIddictAuthorizations",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Scopes",
                table: "OpenIddictAuthorizations",
                newName: "scopes");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictAuthorizations",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictAuthorizations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OpenIddictAuthorizations",
                newName: "creation_date");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictAuthorizations",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "OpenIddictAuthorizations",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                newName: "ix_open_iddict_authorizations_application_id_status_subject_type");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "OpenIddictApplications",
                newName: "settings");

            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "OpenIddictApplications",
                newName: "requirements");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictApplications",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Permissions",
                table: "OpenIddictApplications",
                newName: "permissions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictApplications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RedirectUris",
                table: "OpenIddictApplications",
                newName: "redirect_uris");

            migrationBuilder.RenameColumn(
                name: "PostLogoutRedirectUris",
                table: "OpenIddictApplications",
                newName: "post_logout_redirect_uris");

            migrationBuilder.RenameColumn(
                name: "JsonWebKeySet",
                table: "OpenIddictApplications",
                newName: "json_web_key_set");

            migrationBuilder.RenameColumn(
                name: "DisplayNames",
                table: "OpenIddictApplications",
                newName: "display_names");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "OpenIddictApplications",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "ConsentType",
                table: "OpenIddictApplications",
                newName: "consent_type");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictApplications",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "OpenIddictApplications",
                newName: "client_type");

            migrationBuilder.RenameColumn(
                name: "ClientSecret",
                table: "OpenIddictApplications",
                newName: "client_secret");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "OpenIddictApplications",
                newName: "client_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationType",
                table: "OpenIddictApplications",
                newName: "application_type");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictApplications_ClientId",
                table: "OpenIddictApplications",
                newName: "ix_open_iddict_applications_client_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_roles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_roles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "user_roles",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "user_roles",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "user_roles",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "user_roles",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "user_roles",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "AssignedBy",
                table: "user_roles",
                newName: "assigned_by");

            migrationBuilder.RenameColumn(
                name: "AssignedAt",
                table: "user_roles",
                newName: "assigned_at");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "user_roles",
                newName: "ix_user_roles_user_id_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "user_roles",
                newName: "ix_user_roles_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_Id",
                table: "user_roles",
                newName: "ix_user_roles_id");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "refresh_tokens",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "refresh_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "refresh_tokens",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "refresh_tokens",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "RevokedBy",
                table: "refresh_tokens",
                newName: "revoked_by");

            migrationBuilder.RenameColumn(
                name: "RevokedAt",
                table: "refresh_tokens",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "IsRevoked",
                table: "refresh_tokens",
                newName: "is_revoked");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "refresh_tokens",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "refresh_tokens",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "refresh_tokens",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "refresh_tokens",
                newName: "create_by");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Token",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Id",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "o_auth_clients",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Platform",
                table: "o_auth_clients",
                newName: "platform");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "o_auth_clients",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "o_auth_clients",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "o_auth_clients",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "o_auth_clients",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "o_auth_clients",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "RequirePkce",
                table: "o_auth_clients",
                newName: "require_pkce");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenLifetime",
                table: "o_auth_clients",
                newName: "refresh_token_lifetime");

            migrationBuilder.RenameColumn(
                name: "RedirectUris",
                table: "o_auth_clients",
                newName: "redirect_uris");

            migrationBuilder.RenameColumn(
                name: "PostLogoutRedirectUris",
                table: "o_auth_clients",
                newName: "post_logout_redirect_uris");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "o_auth_clients",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "o_auth_clients",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "o_auth_clients",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "o_auth_clients",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "ClientSecretHash",
                table: "o_auth_clients",
                newName: "client_secret_hash");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "o_auth_clients",
                newName: "client_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationUrl",
                table: "o_auth_clients",
                newName: "application_url");

            migrationBuilder.RenameColumn(
                name: "AllowedScopes",
                table: "o_auth_clients",
                newName: "allowed_scopes");

            migrationBuilder.RenameColumn(
                name: "AllowRefreshTokens",
                table: "o_auth_clients",
                newName: "allow_refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "AccessTokenLifetime",
                table: "o_auth_clients",
                newName: "access_token_lifetime");

            migrationBuilder.RenameIndex(
                name: "IX_OAuthClients_Id",
                table: "o_auth_clients",
                newName: "ix_o_auth_clients_id");

            migrationBuilder.RenameIndex(
                name: "IX_OAuthClients_ClientId",
                table: "o_auth_clients",
                newName: "ix_o_auth_clients_client_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "api_keys",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Scopes",
                table: "api_keys",
                newName: "scopes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "api_keys",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "api_keys",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "api_keys",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UsageCount",
                table: "api_keys",
                newName: "usage_count");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "api_keys",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "api_keys",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "LastUsedAt",
                table: "api_keys",
                newName: "last_used_at");

            migrationBuilder.RenameColumn(
                name: "KeyHash",
                table: "api_keys",
                newName: "key_hash");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "api_keys",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "api_keys",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "api_keys",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "api_keys",
                newName: "create_by");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKeys_UserId",
                table: "api_keys",
                newName: "ix_api_keys_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "api_keys",
                newName: "ix_api_keys_key_hash");

            migrationBuilder.RenameIndex(
                name: "IX_ApiKeys_Id",
                table: "api_keys",
                newName: "ix_api_keys_id");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "picture_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "api_keys",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "key_prefix",
                table: "api_keys",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_roles",
                table: "user_roles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_o_auth_clients",
                table: "o_auth_clients",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_api_keys",
                table: "api_keys",
                column: "id");

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_key_prefix",
                table: "api_keys",
                column: "key_prefix");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_id",
                table: "user_logins",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_provider_provider_user_id",
                table: "user_logins",
                columns: new[] { "provider", "provider_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_api_keys_users_user_id",
                table: "api_keys",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens",
                column: "authorization_id",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id",
                table: "user_roles",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_users_user_id",
                table: "user_roles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_api_keys_users_user_id",
                table: "api_keys");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_users_user_id",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_roles",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_o_auth_clients",
                table: "o_auth_clients");

            migrationBuilder.DropPrimaryKey(
                name: "pk_api_keys",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "ix_api_keys_key_prefix",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "picture_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "description",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "key_prefix",
                table: "api_keys");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "user_roles",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "o_auth_clients",
                newName: "OAuthClients");

            migrationBuilder.RenameTable(
                name: "api_keys",
                newName: "ApiKeys");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "Users",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "last_login_at",
                table: "Users",
                newName: "LastLoginAt");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Users",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "google_id",
                table: "Users",
                newName: "GoogleId");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "Users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "email_confirmed",
                table: "Users",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "Users",
                newName: "CreateBy");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "Users",
                newName: "AvatarUrl");

            migrationBuilder.RenameIndex(
                name: "ix_users_username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "ix_users_id",
                table: "Users",
                newName: "IX_Users_Id");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "ix_users_google_id",
                table: "Users",
                newName: "IX_Users_GoogleId");

            migrationBuilder.RenameColumn(
                name: "permissions",
                table: "Roles",
                newName: "Permissions");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Roles",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Roles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "Roles",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Roles",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Roles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "Roles",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "ix_roles_name",
                table: "Roles",
                newName: "IX_Roles_Name");

            migrationBuilder.RenameIndex(
                name: "ix_roles_id",
                table: "Roles",
                newName: "IX_Roles_Id");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OpenIddictTokens",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "OpenIddictTokens",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OpenIddictTokens",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictTokens",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "payload",
                table: "OpenIddictTokens",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictTokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reference_id",
                table: "OpenIddictTokens",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "redemption_date",
                table: "OpenIddictTokens",
                newName: "RedemptionDate");

            migrationBuilder.RenameColumn(
                name: "expiration_date",
                table: "OpenIddictTokens",
                newName: "ExpirationDate");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "OpenIddictTokens",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictTokens",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "authorization_id",
                table: "OpenIddictTokens",
                newName: "AuthorizationId");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "OpenIddictTokens",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_reference_id",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_AuthorizationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type");

            migrationBuilder.RenameColumn(
                name: "resources",
                table: "OpenIddictScopes",
                newName: "Resources");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictScopes",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "OpenIddictScopes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "descriptions",
                table: "OpenIddictScopes",
                newName: "Descriptions");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "OpenIddictScopes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictScopes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "display_names",
                table: "OpenIddictScopes",
                newName: "DisplayNames");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "OpenIddictScopes",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictScopes",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_scopes_name",
                table: "OpenIddictScopes",
                newName: "IX_OpenIddictScopes_Name");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OpenIddictAuthorizations",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "OpenIddictAuthorizations",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OpenIddictAuthorizations",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scopes",
                table: "OpenIddictAuthorizations",
                newName: "Scopes");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictAuthorizations",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictAuthorizations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "OpenIddictAuthorizations",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictAuthorizations",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "OpenIddictAuthorizations",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                table: "OpenIddictAuthorizations",
                newName: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type");

            migrationBuilder.RenameColumn(
                name: "settings",
                table: "OpenIddictApplications",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "requirements",
                table: "OpenIddictApplications",
                newName: "Requirements");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictApplications",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "permissions",
                table: "OpenIddictApplications",
                newName: "Permissions");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictApplications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "redirect_uris",
                table: "OpenIddictApplications",
                newName: "RedirectUris");

            migrationBuilder.RenameColumn(
                name: "post_logout_redirect_uris",
                table: "OpenIddictApplications",
                newName: "PostLogoutRedirectUris");

            migrationBuilder.RenameColumn(
                name: "json_web_key_set",
                table: "OpenIddictApplications",
                newName: "JsonWebKeySet");

            migrationBuilder.RenameColumn(
                name: "display_names",
                table: "OpenIddictApplications",
                newName: "DisplayNames");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "OpenIddictApplications",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "consent_type",
                table: "OpenIddictApplications",
                newName: "ConsentType");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictApplications",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "client_type",
                table: "OpenIddictApplications",
                newName: "ClientType");

            migrationBuilder.RenameColumn(
                name: "client_secret",
                table: "OpenIddictApplications",
                newName: "ClientSecret");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "OpenIddictApplications",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "application_type",
                table: "OpenIddictApplications",
                newName: "ApplicationType");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_applications_client_id",
                table: "OpenIddictApplications",
                newName: "IX_OpenIddictApplications_ClientId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UserRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserRoles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "UserRoles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "UserRoles",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "UserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "UserRoles",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "UserRoles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "UserRoles",
                newName: "CreateBy");

            migrationBuilder.RenameColumn(
                name: "assigned_by",
                table: "UserRoles",
                newName: "AssignedBy");

            migrationBuilder.RenameColumn(
                name: "assigned_at",
                table: "UserRoles",
                newName: "AssignedAt");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_user_id_role_id",
                table: "UserRoles",
                newName: "IX_UserRoles_UserId_RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_role_id",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_id",
                table: "UserRoles",
                newName: "IX_UserRoles_Id");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "RefreshTokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "RefreshTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "RefreshTokens",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "RefreshTokens",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "revoked_by",
                table: "RefreshTokens",
                newName: "RevokedBy");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "RefreshTokens",
                newName: "RevokedAt");

            migrationBuilder.RenameColumn(
                name: "is_revoked",
                table: "RefreshTokens",
                newName: "IsRevoked");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "RefreshTokens",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "RefreshTokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "RefreshTokens",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "RefreshTokens",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_user_id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Token");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Id");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OAuthClients",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "platform",
                table: "OAuthClients",
                newName: "Platform");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "OAuthClients",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "OAuthClients",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OAuthClients",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "OAuthClients",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "OAuthClients",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "require_pkce",
                table: "OAuthClients",
                newName: "RequirePkce");

            migrationBuilder.RenameColumn(
                name: "refresh_token_lifetime",
                table: "OAuthClients",
                newName: "RefreshTokenLifetime");

            migrationBuilder.RenameColumn(
                name: "redirect_uris",
                table: "OAuthClients",
                newName: "RedirectUris");

            migrationBuilder.RenameColumn(
                name: "post_logout_redirect_uris",
                table: "OAuthClients",
                newName: "PostLogoutRedirectUris");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "OAuthClients",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "OAuthClients",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "OAuthClients",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "OAuthClients",
                newName: "CreateBy");

            migrationBuilder.RenameColumn(
                name: "client_secret_hash",
                table: "OAuthClients",
                newName: "ClientSecretHash");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "OAuthClients",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "application_url",
                table: "OAuthClients",
                newName: "ApplicationUrl");

            migrationBuilder.RenameColumn(
                name: "allowed_scopes",
                table: "OAuthClients",
                newName: "AllowedScopes");

            migrationBuilder.RenameColumn(
                name: "allow_refresh_tokens",
                table: "OAuthClients",
                newName: "AllowRefreshTokens");

            migrationBuilder.RenameColumn(
                name: "access_token_lifetime",
                table: "OAuthClients",
                newName: "AccessTokenLifetime");

            migrationBuilder.RenameIndex(
                name: "ix_o_auth_clients_id",
                table: "OAuthClients",
                newName: "IX_OAuthClients_Id");

            migrationBuilder.RenameIndex(
                name: "ix_o_auth_clients_client_id",
                table: "OAuthClients",
                newName: "IX_OAuthClients_ClientId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ApiKeys",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scopes",
                table: "ApiKeys",
                newName: "Scopes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "ApiKeys",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ApiKeys",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "ApiKeys",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "usage_count",
                table: "ApiKeys",
                newName: "UsageCount");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ApiKeys",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "ApiKeys",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "last_used_at",
                table: "ApiKeys",
                newName: "LastUsedAt");

            migrationBuilder.RenameColumn(
                name: "key_hash",
                table: "ApiKeys",
                newName: "KeyHash");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "ApiKeys",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "ApiKeys",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ApiKeys",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "ApiKeys",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "ix_api_keys_user_id",
                table: "ApiKeys",
                newName: "IX_ApiKeys_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_api_keys_key_hash",
                table: "ApiKeys",
                newName: "IX_ApiKeys_KeyHash");

            migrationBuilder.RenameIndex(
                name: "ix_api_keys_id",
                table: "ApiKeys",
                newName: "IX_ApiKeys_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictTokens",
                table: "OpenIddictTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictScopes",
                table: "OpenIddictScopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictAuthorizations",
                table: "OpenIddictAuthorizations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthClients",
                table: "OAuthClients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiKeys",
                table: "ApiKeys",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKeys_Users_UserId",
                table: "ApiKeys",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                table: "OpenIddictAuthorizations",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                table: "OpenIddictTokens",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
