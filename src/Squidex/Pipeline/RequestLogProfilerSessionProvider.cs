// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Pipeline
{
    public sealed class RequestLogProfilerSessionProvider : ILogProfilerSessionProvider
    {
        private const string ItemKey = "ProfilerSesison";
        private readonly IHttpContextAccessor httpContextAccessor;

        public RequestLogProfilerSessionProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;

            Profile.Init(this);
        }

        public ProfilerSession GetSession()
        {
            var context = httpContextAccessor?.HttpContext?.Items[ItemKey] as ProfilerSession;

            return context;
        }

        public void Start(HttpContext httpContext, ProfilerSession session)
        {
            Guard.NotNull(httpContext, nameof(httpContext));

            httpContext.Items[ItemKey] = session;
        }
    }
}
