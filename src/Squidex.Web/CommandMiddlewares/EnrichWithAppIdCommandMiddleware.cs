// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares
{
    public sealed class EnrichWithAppIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IContextProvider contextProvider;

        public EnrichWithAppIdCommandMiddleware(IContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is IAppCommand appCommand && appCommand.AppId == null)
            {
                var appId = GetAppId();

                appCommand.AppId = appId;
            }

            if (context.Command is AppCommand appSelfCommand && appSelfCommand.AppId == Guid.Empty)
            {
                var appId = GetAppId();

                appSelfCommand.AppId = appId.Id;
            }

            return next();
        }

        private NamedId<Guid> GetAppId()
        {
            var context = contextProvider.Context;

            if (context.App == null)
            {
                throw new InvalidOperationException("Cannot resolve app.");
            }

            return context.App.NamedId();
        }
    }
}