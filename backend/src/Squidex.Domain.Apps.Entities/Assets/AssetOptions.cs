// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetOptions
    {
        public int DefaultPageSize { get; set; } = 200;

        public int MaxResults { get; set; } = 200;

        public long MaxSize { get; set; } = 5 * 1024 * 1024;
    }
}
