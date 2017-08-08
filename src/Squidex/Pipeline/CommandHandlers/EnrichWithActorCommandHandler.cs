// ==========================================================================
//  EnrichWithActorCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Write;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Security;

// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public class EnrichWithActorCommandHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithActorCommandHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is SquidexCommand squidexCommand && squidexCommand.Actor == null)
            {
                var actorToken =
                    FindActorFromSubject() ??
                    FindActorFromClient();

#pragma warning disable
                if (actorToken == null)
                {
                    throw new SecurityException("No actor with subject or client id available");
                }
#pragma warning enable

                squidexCommand.Actor = actorToken;
            }

            return next();
        }

        private RefToken FindActorFromSubject()
        {
            var subjectId = httpContextAccessor.HttpContext.User.OpenIdSubject();

            return subjectId == null ? null : new RefToken("subject", subjectId);
        }

        private RefToken FindActorFromClient()
        {
            var clientId = httpContextAccessor.HttpContext.User.OpenIdClientId();

            return clientId == null ? null : new RefToken("client", clientId);
        }
    }
}
