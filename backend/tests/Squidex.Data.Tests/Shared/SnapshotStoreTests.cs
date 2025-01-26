// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Shared;

public abstract class SnapshotStoreTests : SnapshotStoreTests<SnapshotValue>
{
    protected override SnapshotValue CreateEntity(DomainId id, int version)
    {
        return new SnapshotValue { Value = $"{id}_{version}" };
    }
}
