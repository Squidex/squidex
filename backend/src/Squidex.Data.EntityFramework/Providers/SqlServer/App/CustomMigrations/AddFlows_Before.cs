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
[Migration("20250504173122_AddFlows_Before")]
internal class AddFlows_Before : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_MessagingData",
            table: "MessagingData");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Chats",
            table: "Chats");

        migrationBuilder.DropPrimaryKey(
            name: "PK_AssetKeyValueStore_TusMetadata",
            table: "AssetKeyValueStore_TusMetadata");
    }
}
