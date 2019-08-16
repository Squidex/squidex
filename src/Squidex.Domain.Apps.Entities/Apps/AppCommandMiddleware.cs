// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppCommandMiddleware : GrainCommandMiddleware<AppCommand, IAppGrain>
    {
        private readonly IContextProvider contextProvider;

        public AppCommandMiddleware(IGrainFactory grainFactory, IContextProvider contextProvider)
            : base(grainFactory)
        {
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            await ExecuteCommandAsync(context);

            if (context.PlainResult is IAppEntity app)
            {
                contextProvider.Context.App = app;
            }

            await next();
        }
    }
}
