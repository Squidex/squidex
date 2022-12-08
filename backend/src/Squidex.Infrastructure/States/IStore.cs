﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States;

public interface IStore<T> : IPersistenceFactory<T>
{
    IBatchContext<T> WithBatchContext(Type owner);

    Task ClearSnapshotsAsync();
}
