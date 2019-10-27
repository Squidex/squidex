// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain
{
    public static class HistoryServices
    {
        public static void AddSquidexHistory(this IServiceCollection services)
        {
            services.AddSingletonAs<HistoryService>()
                .As<IEventConsumer>().As<IHistoryService>();
        }
    }
}