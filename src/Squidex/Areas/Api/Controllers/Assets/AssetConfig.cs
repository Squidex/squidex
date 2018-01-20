// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Assets
{
    public sealed class AssetConfig
    {
        public long MaxSize { get; set; } = 5 * 1024 * 1024;
    }
}
