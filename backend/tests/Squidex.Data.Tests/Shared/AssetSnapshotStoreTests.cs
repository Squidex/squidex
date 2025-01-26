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

public abstract class AssetSnapshotStoreTests : SnapshotStoreTests<Asset>
{
    protected override Asset CreateEntity(DomainId id, int version)
    {
        var context = new GivenContext();

        return Cleanup(context.CreateAsset() with { Id = id, Version = version });
    }

    protected override Asset Cleanup(Asset expected)
    {
        return SimpleMapper.Map(expected, new Asset());
    }
}
