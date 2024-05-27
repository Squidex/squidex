// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class UploadAppImageDto : UploadModel
{
    public async Task<UploadAppImage> ToCommandAsync(HttpContext httpContext)
    {
        var file = await ToFileAsync(httpContext, null);

        return SimpleMapper.Map(this, new UploadAppImage { File = file });
    }
}
