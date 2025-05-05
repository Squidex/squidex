// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Squidex.Providers.SqlServer.App.CustomMigrations;

[DbContext(typeof(SqlServerAppDbContext))]
[Migration("20250504173124_AddFlows_After")]
internal class AddFlows_After : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddPrimaryKey(
            name: "PK_MessagingData",
            table: "MessagingData",
            columns: ["Group", "Key"]);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Chats",
            table: "Chats",
            column: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AssetKeyValueStore_TusMetadata",
            table: "AssetKeyValueStore_TusMetadata",
            column: "Key");
    }
}
