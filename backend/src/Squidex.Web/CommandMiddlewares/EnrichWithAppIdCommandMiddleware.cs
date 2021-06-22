// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
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

        public Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is IAppCommand appCommand && appCommand.AppId == null)
            {
                var appId = GetAppId();

                appCommand.AppId = appId;
            }

            return next(context);
        }

        private NamedId<DomainId> GetAppId()
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