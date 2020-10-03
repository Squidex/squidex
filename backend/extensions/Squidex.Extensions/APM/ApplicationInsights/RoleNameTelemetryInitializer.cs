// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace Squidex.Extensions.APM.ApplicationInsights
{
    public sealed class RoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string roleName;

        public RoleNameTelemetryInitializer(IConfiguration configuration)
        {
            roleName = configuration.GetValue<string>("logging:roleName");

            if (string.IsNullOrWhiteSpace(roleName))
            {
                roleName = "Squidex";
            }
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = roleName;
            }
        }
    }
}
