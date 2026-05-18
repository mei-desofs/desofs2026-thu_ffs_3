CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "AuditLogs" (
    "Id" uuid NOT NULL,
    "EventType" character varying(60) NOT NULL,
    "UserId" uuid,
    "TargetResourceId" uuid,
    "TargetResourceType" character varying(80) NOT NULL,
    "IpAddress" character varying(45) NOT NULL,
    "UserAgent" character varying(256) NOT NULL,
    "TimestampUtc" timestamp with time zone NOT NULL,
    "Success" boolean NOT NULL,
    "Details" character varying(1000) NOT NULL,
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PasswordHash" character varying(500) NOT NULL,
    "Role" character varying(20) NOT NULL,
    "IsActive" boolean NOT NULL,
    "FailedLoginAttempts" integer NOT NULL,
    "LockoutUntilUtc" timestamp with time zone,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    "LastLoginAtUtc" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TokenHash" character varying(64) NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    "ExpiresAtUtc" timestamp with time zone NOT NULL,
    "IsRevoked" boolean NOT NULL,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Vaults" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(400) NOT NULL,
    "OwnerId" uuid NOT NULL,
    "DirectoryPath" character varying(600) NOT NULL,
    "RetentionDays" integer NOT NULL,
    "AutoDeleteOnExpiry" boolean NOT NULL,
    "IsArchived" boolean NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Vaults" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Vaults_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Documents" (
    "Id" uuid NOT NULL,
    "VaultId" uuid NOT NULL,
    "UploadedBy" uuid NOT NULL,
    "OriginalFileName" character varying(255) NOT NULL,
    "StoredFileName" character varying(255) NOT NULL,
    "FilePath" character varying(700) NOT NULL,
    "MimeType" character varying(100) NOT NULL,
    "FileSize" bigint NOT NULL,
    "Sha256Hash" character varying(64) NOT NULL,
    "Classification" character varying(20) NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAtUtc" timestamp with time zone,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Documents_Vaults_VaultId" FOREIGN KEY ("VaultId") REFERENCES "Vaults" ("Id") ON DELETE CASCADE
);

CREATE TABLE "VaultAccesses" (
    "Id" uuid NOT NULL,
    "VaultId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "GrantedBy" uuid NOT NULL,
    "GrantedAtUtc" timestamp with time zone NOT NULL,
    "AccessLevel" character varying(20) NOT NULL,
    CONSTRAINT "PK_VaultAccesses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_VaultAccesses_Vaults_VaultId" FOREIGN KEY ("VaultId") REFERENCES "Vaults" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DocumentVersions" (
    "Id" uuid NOT NULL,
    "DocumentId" uuid NOT NULL,
    "VersionNumber" integer NOT NULL,
    "StoredFileName" character varying(255) NOT NULL,
    "Sha256Hash" character varying(64) NOT NULL,
    "UploadedBy" uuid NOT NULL,
    "FileSize" bigint NOT NULL,
    "UploadedAtUtc" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_DocumentVersions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DocumentVersions_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AuditLogs_TimestampUtc" ON "AuditLogs" ("TimestampUtc");

CREATE INDEX "IX_Documents_VaultId" ON "Documents" ("VaultId");

CREATE INDEX "IX_DocumentVersions_DocumentId" ON "DocumentVersions" ("DocumentId");

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE UNIQUE INDEX "IX_VaultAccesses_VaultId_UserId" ON "VaultAccesses" ("VaultId", "UserId");

CREATE INDEX "IX_Vaults_OwnerId" ON "Vaults" ("OwnerId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260518143506_Initial', '9.0.0');

COMMIT;

