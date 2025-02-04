﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Squidex.Providers.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetFolders",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IndexedAppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ParentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FolderName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetFolders", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "AssetKeyValueStore_TusMetadata",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetKeyValueStore_TusMetadata", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IndexedAppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ParentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileVersion = table.Column<long>(type: "bigint", nullable: false),
                    TotalSize = table.Column<long>(type: "bigint", nullable: false),
                    IsProtected = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "Cache",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cache", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentReferencesAll",
                columns: table => new
                {
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FromKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ToId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentReferencesAll", x => new { x.AppId, x.FromKey, x.ToId });
                });

            migrationBuilder.CreateTable(
                name: "ContentReferencesPublished",
                columns: table => new
                {
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FromKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ToId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentReferencesPublished", x => new { x.AppId, x.FromKey, x.ToId });
                });

            migrationBuilder.CreateTable(
                name: "ContentsAll",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    ScheduleJob = table.Column<string>(type: "jsonb", nullable: true),
                    IndexedAppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IndexedSchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NewData = table.Column<string>(type: "jsonb", nullable: true),
                    TranslationStatus = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentsAll", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "ContentsPublished",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    ScheduleJob = table.Column<string>(type: "jsonb", nullable: true),
                    IndexedAppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IndexedSchemaId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NewData = table.Column<string>(type: "jsonb", nullable: true),
                    TranslationStatus = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentsPublished", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "Counter",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CounterKey = table.Column<string>(type: "text", nullable: false),
                    CounterValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counter", x => new { x.Key, x.Date, x.Category, x.CounterKey });
                });

            migrationBuilder.CreateTable(
                name: "EventPosition",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPosition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventStream = table.Column<string>(type: "text", nullable: false),
                    EventStreamOffset = table.Column<long>(type: "bigint", nullable: false),
                    EventsCount = table.Column<long>(type: "bigint", nullable: false),
                    Events = table.Column<string[]>(type: "text[]", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoryEvent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Actor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Parameters = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ChannelName = table.Column<string>(type: "text", nullable: false),
                    QueueName = table.Column<string>(type: "text", nullable: false),
                    MessageData = table.Column<byte[]>(type: "bytea", nullable: false),
                    MessageHeaders = table.Column<string>(type: "text", nullable: false),
                    TimeToLive = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeHandled = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessagingData",
                columns: table => new
                {
                    Group = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    ValueType = table.Column<string>(type: "text", nullable: false),
                    ValueFormat = table.Column<string>(type: "text", nullable: true),
                    ValueData = table.Column<byte[]>(type: "bytea", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagingData", x => new { x.Group, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Migrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Migrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ApplicationId = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Properties = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RuleId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JobResult = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Job = table.Column<string>(type: "jsonb", nullable: false),
                    LastDump = table.Column<string>(type: "text", nullable: true),
                    NumCalls = table.Column<int>(type: "integer", nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NextAttempt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "States_App",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserIds = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_App", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Counters",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Counters", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_EventConsumerState",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_EventConsumerState", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Identity_Keys",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Identity_Keys", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Identity_Xml",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Identity_Xml", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Index_TagHistory",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Index_TagHistory", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Index_Tags",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Index_Tags", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_JobsState",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_JobsState", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Names",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Names", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Rule",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Rule", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Schema",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AppId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Schema", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_Team",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserIds = table.Column<string>(type: "text", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    AuthDomain = table.Column<string>(type: "text", nullable: true),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_Team", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_UISettings",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_UISettings", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_UsageNotifications",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_UsageNotifications", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "States_UsageTracker",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Document = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States_UsageTracker", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "TextState",
                columns: table => new
                {
                    UniqueContentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextState", x => x.UniqueContentId);
                });

            migrationBuilder.CreateTable(
                name: "YDotNetDocument",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YDotNetDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ApplicationId = table.Column<string>(type: "text", nullable: true),
                    AuthorizationId = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetFolders_IndexedAppId_Id",
                table: "AssetFolders",
                columns: new[] { "IndexedAppId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetKeyValueStore_TusMetadata_Expires",
                table: "AssetKeyValueStore_TusMetadata",
                column: "Expires");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_IndexedAppId_Id",
                table: "Assets",
                columns: new[] { "IndexedAppId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_LastUpdated",
                table: "Chats",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventStream_EventStreamOffset",
                table: "Events",
                columns: new[] { "EventStream", "EventStreamOffset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventStream_Position",
                table: "Events",
                columns: new[] { "EventStream", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventStream_Timestamp",
                table: "Events",
                columns: new[] { "EventStream", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelName_TimeHandled",
                table: "Messages",
                columns: new[] { "ChannelName", "TimeHandled" });

            migrationBuilder.CreateIndex(
                name: "IX_MessagingData_Expiration",
                table: "MessagingData",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Key",
                table: "Requests",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssetFolders");

            migrationBuilder.DropTable(
                name: "AssetKeyValueStore_TusMetadata");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Cache");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "ContentReferencesAll");

            migrationBuilder.DropTable(
                name: "ContentReferencesPublished");

            migrationBuilder.DropTable(
                name: "ContentsAll");

            migrationBuilder.DropTable(
                name: "ContentsPublished");

            migrationBuilder.DropTable(
                name: "Counter");

            migrationBuilder.DropTable(
                name: "EventPosition");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "HistoryEvent");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "MessagingData");

            migrationBuilder.DropTable(
                name: "Migrations");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "RuleEvents");

            migrationBuilder.DropTable(
                name: "States_App");

            migrationBuilder.DropTable(
                name: "States_Counters");

            migrationBuilder.DropTable(
                name: "States_EventConsumerState");

            migrationBuilder.DropTable(
                name: "States_Identity_Keys");

            migrationBuilder.DropTable(
                name: "States_Identity_Xml");

            migrationBuilder.DropTable(
                name: "States_Index_TagHistory");

            migrationBuilder.DropTable(
                name: "States_Index_Tags");

            migrationBuilder.DropTable(
                name: "States_JobsState");

            migrationBuilder.DropTable(
                name: "States_Names");

            migrationBuilder.DropTable(
                name: "States_Rule");

            migrationBuilder.DropTable(
                name: "States_Schema");

            migrationBuilder.DropTable(
                name: "States_Team");

            migrationBuilder.DropTable(
                name: "States_UISettings");

            migrationBuilder.DropTable(
                name: "States_UsageNotifications");

            migrationBuilder.DropTable(
                name: "States_UsageTracker");

            migrationBuilder.DropTable(
                name: "TextState");

            migrationBuilder.DropTable(
                name: "YDotNetDocument");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");
        }
    }
}
