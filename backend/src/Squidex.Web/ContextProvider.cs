// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;

namespace Squidex.Web;

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
                    asyncLocal.Value = Context.Anonymous(null!);
                }

                return asyncLocal.Value;
            }

            return httpContextAccessor.HttpContext.Context();
        }
    }

    public ContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }
}
