// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

[OpenApiRequest]
public sealed class UpdateAssetDto : UploadModel
{
    public async Task<UpdateAsset> ToCommandAsync(DomainId id, HttpContext httpContext, App app)
    {
        var file = await ToFileAsync(httpContext, app);

        return SimpleMapper.Map(this, new UpdateAsset { AssetId = id, File = file });
    }
}
