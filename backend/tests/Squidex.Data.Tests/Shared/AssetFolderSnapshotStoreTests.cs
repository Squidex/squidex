// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Shared;

public abstract class AssetFolderSnapshotStoreTests : SnapshotStoreTests<AssetFolder>
{
    protected override AssetFolder CreateEntity(DomainId id, int version)
    {
        var context = new GivenContext();

        return Cleanup(context.CreateAssetFolder() with { Id = id, Version = version });
    }

    protected override AssetFolder Cleanup(AssetFolder expected)
    {
        return SimpleMapper.Map(expected, new AssetFolder());
    }
}
