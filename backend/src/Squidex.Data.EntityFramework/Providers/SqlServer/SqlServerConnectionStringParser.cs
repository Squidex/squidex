// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerConnectionStringParser : ConnectionStringParser
{
    protected override string? GetProviderSpecificHostName(string source)
    {
        var builder = new SqlConnectionStringBuilder(source);

        return builder.DataSource;
    }
}
