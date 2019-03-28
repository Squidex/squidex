// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AssetRequestSizeLimitAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public int Order { get; set; } = 900;

        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var assetOptions = serviceProvider.GetService<IOptions<AssetOptions>>();

            if (assetOptions?.Value.MaxSize > 0)
            {
                var filter = serviceProvider.GetRequiredService<RequestSizeLimitFilter>();

                filter.Bytes = assetOptions.Value.MaxSize;

                return filter;
            }
            else
            {
                var filter = serviceProvider.GetRequiredService<DisableRequestSizeLimitFilter>();

                return filter;
            }
        }
    }
}
