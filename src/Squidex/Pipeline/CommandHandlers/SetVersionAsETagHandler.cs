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
            var result = context.Result<object>() as EntitySavedResult;

            if (result != null)
            {
                httpContextAccessor.HttpContext.Response.Headers["ETag"] = new StringValues(result.Version.ToString());
            }

            return TaskHelper.False;
        }
    }
}
