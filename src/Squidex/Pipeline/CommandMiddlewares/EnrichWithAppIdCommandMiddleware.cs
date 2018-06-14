// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public sealed class EnrichWithAppIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithAppIdCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return next();
            }

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
            var appFeature = httpContextAccessor.HttpContext.Features.Get<IAppFeature>();

            if (appFeature?.App == null)
            {
                throw new InvalidOperationException("Cannot resolve app.");
            }

            return NamedId.Of(appFeature.App.Id, appFeature.App.Name);
        }
    }
}