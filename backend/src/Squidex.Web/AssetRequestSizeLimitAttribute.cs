// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Web;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AssetRequestSizeLimitAttribute : Attribute, IAuthorizationFilter, IRequestSizePolicy, IOrderedFilter
{
    public int Order { get; } = 900;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var assetOptions = context.HttpContext.RequestServices.GetService<IOptions<AssetOptions>>();

        var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

        if (maxRequestBodySizeFeature?.IsReadOnly == false)
        {
            if (assetOptions?.Value.MaxSize > 0)
            {
                maxRequestBodySizeFeature.MaxRequestBodySize = assetOptions.Value.MaxSize;
            }
            else
            {
                maxRequestBodySizeFeature.MaxRequestBodySize = null;
            }
        }

        if (assetOptions?.Value.MaxSize > 0)
        {
            var options = new FormOptions
            {
                MultipartBodyLengthLimit = assetOptions.Value.MaxSize
            };

            context.HttpContext.Features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, options));
        }
    }
}
