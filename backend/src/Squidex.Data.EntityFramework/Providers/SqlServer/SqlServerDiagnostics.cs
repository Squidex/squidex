// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenTelemetry.Trace;
using Squidex.Infrastructure;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerDiagnostics : ITelemetryConfigurator
{
    public void Configure(TracerProviderBuilder builder)
    {
    }
}
