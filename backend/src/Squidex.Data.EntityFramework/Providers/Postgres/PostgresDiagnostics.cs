// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Npgsql;
using OpenTelemetry.Trace;
using Squidex.Infrastructure;

namespace Squidex.Providers.Postgres;

public sealed class PostgresDiagnostics : ITelemetryConfigurator
{
    public void Configure(TracerProviderBuilder builder)
    {
        builder.AddNpgsql();
    }
}
