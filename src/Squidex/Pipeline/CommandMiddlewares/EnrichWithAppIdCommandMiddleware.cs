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
            if (context.Command is IAppCommand appCommand && appCommand.AppId == null)
            {
                var appFeature = httpContextAccessor.HttpContext.Features.Get<IAppFeature>();

                if (appFeature == null)
                {
                    throw new InvalidOperationException("Cannot resolve app.");
                }

                appCommand.AppId = new NamedId<Guid>(appFeature.App.Id, appFeature.App.Name);
            }

            return next();
        }
    }
}
