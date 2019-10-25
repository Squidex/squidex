﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class ConfigurePreviewUrlsDto : Dictionary<string, string>
    {
        public ConfigurePreviewUrls ToCommand()
        {
            return new ConfigurePreviewUrls
            {
                PreviewUrls = new Dictionary<string, string>(this)
            };
        }
    }
}
