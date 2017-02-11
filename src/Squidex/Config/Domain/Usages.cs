// ==========================================================================
//  Usages.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Apps.Services;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Schemas.Services;

namespace Squidex.Config.Domain
{
    public static class Usages
    {
        public static IApplicationBuilder UseMyEventStore(this IApplicationBuilder app)
        {
            var catchConsumers = app.ApplicationServices.GetServices<IEventCatchConsumer>();

            foreach (var catchConsumer in catchConsumers)
            {
                var receiver = app.ApplicationServices.GetService<EventReceiver>();

                receiver?.Subscribe(catchConsumer);
            }

            var appProvider = app.ApplicationServices.GetRequiredService<IAppProvider>();

            app.ApplicationServices.GetRequiredService<IAppRepository>().AppSaved += appId =>
            {
                appProvider.Remove(appId);
            };
            
            var schemaProvider = app.ApplicationServices.GetRequiredService<ISchemaProvider>();

            app.ApplicationServices.GetRequiredService<ISchemaRepository>().SchemaSaved += (appId, schemaId) =>
            {
                schemaProvider.Remove(appId, schemaId);
            };

            return app;
        }

        public static IApplicationBuilder TestExternalSystems(this IApplicationBuilder app)
        {
            var systems = app.ApplicationServices.GetRequiredService<IEnumerable<IExternalSystem>>();

            foreach (var system in systems)
            {
                system.CheckConnection();
            }

            return app;
        }
    }
}
