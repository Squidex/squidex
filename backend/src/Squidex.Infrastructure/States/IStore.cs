// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface IStore<T> : IPersistenceFactory<T>
    {
        IBatchContext<T> WithBatchContext(Type owner);

        Task ClearAsync();
    }
}
