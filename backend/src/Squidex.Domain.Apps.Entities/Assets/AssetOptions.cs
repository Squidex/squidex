// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetOptions
    {
        public bool FolderPerApp { get; set; } = false;

        public int DefaultPageSize { get; set; } = 200;

        public int MaxResults { get; set; } = 200;

        public long MaxSize { get; set; } = 5 * 1024 * 1024;

        public TimeSpan TimeoutFind { get; set; } = TimeSpan.FromSeconds(1);

        public TimeSpan TimeoutQuery { get; set; } = TimeSpan.FromSeconds(5);
    }
}
