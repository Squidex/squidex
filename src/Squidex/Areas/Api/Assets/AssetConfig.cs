// ==========================================================================
//  AssetConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Assets
{
    public sealed class AssetConfig
    {
        public long MaxSize { get; set; } = 5 * 1024 * 1024;
    }
}
