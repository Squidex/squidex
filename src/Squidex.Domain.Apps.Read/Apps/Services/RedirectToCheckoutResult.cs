// ==========================================================================
//  RedirectToCheckoutResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public sealed class RedirectToCheckoutResult : IChangePlanResult
    {
        public Uri Url { get; }

        public RedirectToCheckoutResult(Uri url)
        {
            Guard.NotNull(url, nameof(url));

            Url = url;
        }
    }
}
