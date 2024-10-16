// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Config;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Areas.Api.Controllers;

public class UploadModel
{
    /// <summary>
    /// The file to upload.
    /// </summary>
    [FromForm(Name = "file")]
    public IFormFile File { get; set; }

    /// <summary>
    /// The alternative URL to download from.
    /// </summary>
    [FromForm(Name = "url")]
    public string? Url { get; set; }

    /// <summary>
    /// The file name if the URL is specified.
    /// </summary>
    [FromForm(Name = "name")]
    public string? Name { get; set; }

    public Task<IAssetFile> ToFileAsync(HttpContext httpContext, App? app)
    {
        var resolver = httpContext.RequestServices.GetRequiredService<AssetFileResolver>();

        return resolver.ToFileAsync(this, httpContext, app,
            httpContext.RequestAborted);
    }
}
