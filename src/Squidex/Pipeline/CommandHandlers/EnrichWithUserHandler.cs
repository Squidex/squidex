// ==========================================================================
//  EnrichWithSubjectHandler.cs
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
// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public class EnrichWithUserHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithUserHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var subjectCommand = context.Command as IUserCommand;

            if (subjectCommand != null)
            {
                var userToken = 
                    FindUserFromSubject() ?? 
                    FindUserFromClient();

                if (userToken == null)
                {
                    throw new SecurityException("No user with subject or client id available");
                }

                subjectCommand.User = userToken;
            }

            return Task.FromResult(false);
        }

        private UserToken FindUserFromSubject()
        {
            var subjectId = httpContextAccessor.HttpContext.User.OpenIdSubject();

            return subjectId == null ? null : new UserToken("subject", subjectId);
        }

        private UserToken FindUserFromClient()
        {
            var clientId = httpContextAccessor.HttpContext.User.OpenIdClientId();

            return clientId == null ? null : new UserToken("client", clientId);
        }
    }
}
