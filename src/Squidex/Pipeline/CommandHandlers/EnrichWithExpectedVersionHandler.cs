// ==========================================================================
//  EnrichWithExpectedVersionHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Pipeline.CommandHandlers
{
    public sealed class EnrichWithExpectedVersionHandler : ICommandHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithExpectedVersionHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            var headers = httpContextAccessor.HttpContext.Request.Headers;
            var headerMatch = headers["If-Match"].ToString();

            if (!string.IsNullOrWhiteSpace(headerMatch) && long.TryParse(headerMatch, NumberStyles.Any, CultureInfo.InvariantCulture, out long expectedVersion))
            {
                context.Command.ExpectedVersion = expectedVersion;
            }

            return TaskHelper.False;
        }
    }
}
