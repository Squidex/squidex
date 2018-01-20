// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Services
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
