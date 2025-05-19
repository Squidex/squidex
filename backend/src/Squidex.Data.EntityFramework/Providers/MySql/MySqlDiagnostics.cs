// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenTelemetry.Trace;
using Squidex.Infrastructure;

namespace Squidex.Providers.MySql;

public sealed class MySqlDiagnostics : ITelemetryConfigurator
{
    public void Configure(TracerProviderBuilder builder)
    {
        builder.AddSource("MySqlConnector");
    }
}
