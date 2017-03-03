// ==========================================================================
//  EnrichWithActorHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Tasks;
using Squidex.Write;

// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public class EnrichWithActorHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithActorHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var squidexCommand = context.Command as SquidexCommand;

            if (squidexCommand != null)
            {
                var actorToken = 
                    FindActorFromSubject() ?? 
                    FindActorFromClient();

                squidexCommand.Actor = actorToken ?? throw new SecurityException("No actor with subject or client id available");
            }

            return TaskHelper.False;
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
