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
                var subjectId = httpContextAccessor.HttpContext.User.OpenIdSubject();

                if (subjectId == null)
                {
                    var clientId = httpContextAccessor.HttpContext.User.OpenIdClientId();

                    if (clientId != null)
                    {
                        subjectId = $"Client:"
                    }
                }

                if (subjectId == null)
                {
                    throw new SecurityException("No user with subject id available");
                }

                subjectCommand.UserId = subjectId;
            }

            return Task.FromResult(false);
        }
    }
}
