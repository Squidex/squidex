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

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerScriptMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer)
    : SqlServerMigrationsSqlGenerator(dependencies, commandBatchPreparer)
{
    protected override void Generate(SqlOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (!Options.HasFlag(MigrationsSqlGenerationOptions.Script))
        {
            base.Generate(operation, model, builder);
            return;
        }

        // Raw statements like functions must be the first statement in a batch, therefore the script would not run in SQL Server clients.
        builder
            .AppendLine("GO")
            .AppendLine(operation.Sql)
            .AppendLine("GO");

        EndStatement(builder, operation.SuppressTransaction);
    }
}
