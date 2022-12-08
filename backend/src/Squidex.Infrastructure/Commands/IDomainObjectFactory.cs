// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands;

public interface IDomainObjectFactory
{
    T Create<T>(DomainId id);

    T Create<T, TState>(DomainId id, IPersistenceFactory<TState> factory);
}
