// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class UpsertAsset : UploadAssetCommand
    {
        public DomainId? ParentId { get; set; }

        public UpsertAsset()
        {
            AssetId = DomainId.NewGuid();
        }

        public CreateAsset AsCreate()
        {
            return SimpleMapper.Map(this, new CreateAsset());
        }

        public UpdateAsset AsUpdate()
        {
            return SimpleMapper.Map(this, new UpdateAsset());
        }

        public MoveAsset AsMove(DomainId parentId)
        {
            return SimpleMapper.Map(this, new MoveAsset { ParentId = parentId });
        }
    }
}
