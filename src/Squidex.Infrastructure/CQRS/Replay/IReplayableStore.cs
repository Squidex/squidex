// ==========================================================================
//  IReplayableStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Replay
{
    public interface IReplayableStore
    {
        Task ClearAsync();
    }
}
