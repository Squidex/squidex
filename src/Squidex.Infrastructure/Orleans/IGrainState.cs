// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Orleans
{
    public interface IGrainState<T> where T : class, new()
    {
        long Version { get; }

        T Value { get; set; }

        Task ClearAsync();

        Task WriteAsync();

        Task WriteEventAsync(Envelope<IEvent> envelope);
    }
}
