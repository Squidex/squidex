// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Squidex.Infrastructure.States;

public sealed class MongoSnapshotStore<T>(IMongoDatabase database) : MongoSnapshotStoreBase<T, MongoState<T>>(database)
{
}
