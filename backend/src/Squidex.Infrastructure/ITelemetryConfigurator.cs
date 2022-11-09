// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using OpenTelemetry.Trace;

namespace Squidex.Infrastructure;

public interface ITelemetryConfigurator
{
    void Configure(TracerProviderBuilder builder)
    {
    }
}
