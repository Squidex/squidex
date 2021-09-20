// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain
{
    public static class AppsServices
    {
        public static void AddSquidexApps(this IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("apps:deletePermanent"))
            {
                services.AddSingletonAs<AppPermanentDeleter>()
                    .As<IEventConsumer>();
            }

            services.AddTransientAs<AppDomainObject>()
                .AsSelf();

            services.AddSingletonAs<RolePermissionsProvider>()
                .AsSelf();

            services.AddSingletonAs<AppEventDeleter>()
                .As<IDeleter>();

            services.AddSingletonAs<AppUsageDeleter>()
                .As<IDeleter>();

            services.AddSingletonAs<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<DefaultAppImageStore>()
                .As<IAppImageStore>();

            services.AddSingletonAs<AppProvider>()
                .As<IAppProvider>();

            services.AddSingletonAs<AppUISettings>()
                .As<IAppUISettings>().As<IDeleter>();

            services.AddSingletonAs<AppSettingsSearchSource>()
                .As<ISearchSource>();

            services.AddSingleton(c =>
            {
                var uiOptions = c.GetRequiredService<IOptions<MyUIOptions>>().Value;

                var patterns = new List<Pattern>();

                if (uiOptions.RegexSuggestions != null)
                {
                    foreach (var (key, value) in uiOptions.RegexSuggestions)
                    {
                        if (!string.IsNullOrWhiteSpace(key) &&
                            !string.IsNullOrWhiteSpace(value))
                        {
                            patterns.Add(new Pattern(key, value));
                        }
                    }
                }

                return new InitialSettings
                {
                    Settings = new AppSettings
                    {
                        Patterns = ImmutableList.ToImmutableList(patterns)
                    }
                };
            });
        }
    }
}
