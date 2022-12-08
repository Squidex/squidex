// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Squidex.Infrastructure.States;

public sealed class MongoSnapshotStore<T> : MongoSnapshotStoreBase<T, MongoState<T>>
{
    public MongoSnapshotStore(IMongoDatabase database)
        : base(database)
    {
    }
}
