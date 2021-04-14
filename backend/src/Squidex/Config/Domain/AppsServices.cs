// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Search;

namespace Squidex.Config.Domain
{
    public static class AppsServices
    {
        public static void AddSquidexApps(this IServiceCollection services)
        {
            services.AddTransientAs<AppDomainObject>()
                .AsSelf();

            services.AddSingletonAs<RolePermissionsProvider>()
                .AsSelf();

            services.AddSingletonAs<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<DefaultAppImageStore>()
                .As<IAppImageStore>();

            services.AddSingletonAs<AppProvider>()
                .As<IAppProvider>();

            services.AddSingletonAs<AppUISettings>()
                .As<IAppUISettings>();

            services.AddSingletonAs<AppSettingsSearchSource>()
                .As<ISearchSource>();

            services.AddSingleton(c =>
            {
                var uiOptions = c.GetRequiredService<IOptions<MyUIOptions>>().Value;

                return new InitialSettings
                {
                    Settings = uiOptions.AppSettings
                };
            });
        }
    }
}