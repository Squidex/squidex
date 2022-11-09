// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Commands;

public sealed class CreateAsset : UploadAssetCommand, IMoveAssetCommand
{
    public DomainId ParentId { get; set; }

    public bool Duplicate { get; set; }

    public bool OptimizeValidation { get; set; }

    public CreateAsset()
    {
        AssetId = DomainId.NewGuid();
    }

    public MoveAsset AsMove()
    {
        return SimpleMapper.Map(this, new MoveAsset());
    }
}
