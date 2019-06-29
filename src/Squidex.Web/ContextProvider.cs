// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;

namespace Squidex.Web
{
    public sealed class ContextProvider : IContextProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public Context Context
        {
            get { return httpContextAccessor.HttpContext.Context(); }
        }

        public ContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

            this.httpContextAccessor = httpContextAccessor;
        }
    }
}
