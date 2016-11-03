// ==========================================================================
//  EnrichWithSubjectHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.CQRS.Commands;
// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public class EnrichWithSubjectHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithSubjectHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var subjectCommand = context.Command as ISubjectCommand;

            if (subjectCommand != null)
            {
                var user = httpContextAccessor.HttpContext.User;

                if (user?.FindFirst(ClaimTypes.NameIdentifier) == null)
                {
                    throw new SecurityException("No user available");
                }

                subjectCommand.SubjectId = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            return Task.FromResult(false);
        }
    }
}
