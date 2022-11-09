// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States;

public interface IBatchContext<T> : IAsyncDisposable, IPersistenceFactory<T>
{
    Task CommitAsync();

    Task LoadAsync(IEnumerable<DomainId> ids);
}
