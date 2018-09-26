// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;

namespace Squidex.Config.Orleans
{
    public static class Extensions
    {
        public static void AddMyParts(this IApplicationPartManager builder)
        {
            builder.AddApplicationPart(SquidexEntities.Assembly);
            builder.AddApplicationPart(SquidexInfrastructure.Assembly);
        }

        public static void Configure(this ClusterOptions options)
        {
            options.ClusterId = Constants.OrleansClusterId;
            options.ServiceId = Constants.OrleansClusterId;
        }
    }
}
