// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.States
{
    public sealed class MongoSnapshotStore<T> : MongoSnapshotStoreBase<T, MongoState<T>>
    {
        public MongoSnapshotStore(IMongoDatabase database, JsonSerializer jsonSerializer)
            : base(database, jsonSerializer)
        {
        }
    }
}
