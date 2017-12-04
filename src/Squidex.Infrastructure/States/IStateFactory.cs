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
        Task<T> GetSynchronizedAsync<T>(string key) where T : IStatefulObject;

        Task<T> GetDetachedAsync<T>(string key) where T : IStatefulObject;
    }
}
