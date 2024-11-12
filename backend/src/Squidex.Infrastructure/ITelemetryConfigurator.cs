// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Squidex.Infrastructure;

public interface ITelemetryConfigurator
{
    void Configure(TracerProviderBuilder builder)
    {
    }

    void Configure(MeterProviderBuilder builder)
    {
    }
}
