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
    public interface IStateFactory
    {
        Task<T> GetSingleAsync<T>(string key) where T : IStatefulObject<string>;

        Task<T> GetSingleAsync<T>(Guid key) where T : IStatefulObject<Guid>;

        Task<T> GetSingleAsync<T, TKey>(TKey key) where T : IStatefulObject<TKey>;

        Task<T> CreateAsync<T>(string key) where T : IStatefulObject<string>;

        Task<T> CreateAsync<T>(Guid key) where T : IStatefulObject<Guid>;

        Task<T> CreateAsync<T, TKey>(TKey key) where T : IStatefulObject<TKey>;

        void Remove<T, TKey>(TKey key) where T : IStatefulObject<TKey>;

        void Synchronize<T, TKey>(TKey key) where T : IStatefulObject<TKey>;
    }
}
