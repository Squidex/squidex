// ==========================================================================
//  RenameAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Write.Assets.Commands
{
    public sealed class RenameAsset : AssetAggregateCommand
    {
        public string FileName { get; set; }
    }
}
