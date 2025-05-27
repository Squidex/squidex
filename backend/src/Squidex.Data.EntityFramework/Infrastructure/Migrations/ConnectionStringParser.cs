// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Data.Common;

namespace Squidex.Infrastructure.Migrations;

public class ConnectionStringParser
{
    public string? GetHostName(string? source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return null;
        }

        try
        {
            return GetProviderSpecificHostName(source);
        }
        catch
        {
            try
            {
                var builder = new DbConnectionStringBuilder
                {
                    ConnectionString = source,
                };

                if (builder.TryGetValue("Server", out var server))
                {
                    return server?.ToString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    protected virtual string? GetProviderSpecificHostName(string source)
    {
        throw new NotFiniteNumberException();
    }
}
