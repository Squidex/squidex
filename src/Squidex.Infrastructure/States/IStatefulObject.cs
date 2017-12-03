// ==========================================================================
//  IStatefulObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.States
{
    public interface IStatefulObject
    {
        Task ActivateAsync(string key, IStore store);
    }
}
