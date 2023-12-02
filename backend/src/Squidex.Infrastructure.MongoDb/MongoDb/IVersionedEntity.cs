// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.MongoDb;

public interface IVersionedEntity<T>
{
    T UniqueId { get; }

    long Version { get; }
}
