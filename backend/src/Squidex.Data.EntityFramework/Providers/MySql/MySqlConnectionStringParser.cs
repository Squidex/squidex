// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MySqlConnector;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Providers.MySql;

public sealed class MySqlConnectionStringParser : ConnectionStringParser
{
    protected override string? GetProviderSpecificHostName(string source)
    {
        var builder = new MySqlConnectionStringBuilder(source);

        return builder.Server;
    }
}
