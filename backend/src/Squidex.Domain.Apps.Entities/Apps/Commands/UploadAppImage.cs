﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UploadAppImage : AppUpdateCommand
    {
        public AssetFile File { get; set; }
    }
}
