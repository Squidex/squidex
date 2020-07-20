// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;

namespace Squidex.Web
{
    public sealed class ContextProvider : IContextProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AsyncLocal<Context> asyncLocal = new AsyncLocal<Context>();

        public Context Context
        {
            get
            {
                if (httpContextAccessor.HttpContext == null)
                {
                    if (asyncLocal.Value == null)
                    {
                        asyncLocal.Value = Context.Anonymous();
                    }

                    return asyncLocal.Value;
                }

                return httpContextAccessor.HttpContext.Context();
            }
        }

        public ContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

            this.httpContextAccessor = httpContextAccessor;
        }
    }
}
