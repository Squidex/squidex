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
    public GivenContext Context { get; } = new GivenContext();

    protected override Asset CreateEntity(DomainId id, int version)
    {
        return Cleanup(Context.CreateAsset() with { Id = id, Version = version });
    }

    protected override Asset Cleanup(Asset expected)
    {
        return SimpleMapper.Map(expected, new Asset());
    }
}
