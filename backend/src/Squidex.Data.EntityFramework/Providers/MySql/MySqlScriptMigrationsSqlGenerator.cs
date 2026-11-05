// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Microting.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Microting.EntityFrameworkCore.MySql.Migrations;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace Squidex.Providers.MySql;

public sealed class MySqlScriptMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer,
    IMySqlOptions options)
    : MySqlMigrationsSqlGenerator(dependencies, commandBatchPreparer, options)
{
    protected override void Generate(SqlOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (!Options.HasFlag(MigrationsSqlGenerationOptions.Script))
        {
            base.Generate(operation, model, builder);
            return;
        }

        // Raw statements like functions contain semicolons and are not terminated, therefore the script would not run in MySQL clients.
        builder
            .AppendLine("DELIMITER //")
            .Append(operation.Sql)
            .AppendLine(" //")
            .AppendLine("DELIMITER ;");

        EndStatement(builder, operation.SuppressTransaction);
    }
}
