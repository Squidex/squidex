// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Collections;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

[OpenApiRequest]
public sealed class ConfigurePreviewUrlsDto : Dictionary<string, string>
{
    public ConfigurePreviewUrls ToCommand()
    {
        return new ConfigurePreviewUrls
        {
            PreviewUrls = new Dictionary<string, string>(this).ToReadonlyDictionary()
        };
    }
}
