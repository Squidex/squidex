// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Npgsql;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Providers.Postgres;

public class PostgresConnectionStringParser : ConnectionStringParser
{
    protected override string? GetProviderSpecificHostName(string source)
    {
        var builder = new NpgsqlConnectionStringBuilder(source);

        return builder.Host;
    }
}
