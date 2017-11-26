// ==========================================================================
//  IStateFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface IStateFactory
    {
        Task<T> GetAsync<T, TState>(string key) where T : StatefulObject<TState>;

        Task<T> GetDetachedAsync<T, TState>(string key) where T : StatefulObject<TState>;
    }
}
