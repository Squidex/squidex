// ==========================================================================
//  SetVersionAsETagHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Pipeline.CommandHandlers
{
    public class SetVersionAsETagHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SetVersionAsETagHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            if (context.Result<object>() is EntitySavedResult result)
            {
                httpContextAccessor.HttpContext.Response.Headers["ETag"] = new StringValues(result.Version.ToString());
            }

            return TaskHelper.False;
        }
    }
}
