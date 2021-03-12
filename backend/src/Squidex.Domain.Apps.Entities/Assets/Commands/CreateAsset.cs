﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class CreateAsset : UploadAssetCommand
    {
        public DomainId ParentId { get; set; }

        public string? ParentPath { get; set; }

        public bool Duplicate { get; set; }

        public CreateAsset()
        {
            AssetId = DomainId.NewGuid();
        }

        public MoveAsset AsMove()
        {
            return SimpleMapper.Map(this, new MoveAsset());
        }
    }
}
