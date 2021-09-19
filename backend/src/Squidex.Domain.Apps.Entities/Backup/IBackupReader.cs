// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupReader : IDisposable
    {
        int ReadAttachments { get; }

        int ReadEvents { get; }

        Task<Stream> OpenBlobAsync(string name,
            CancellationToken ct = default);

        Task<T> ReadJsonAsync<T>(string name,
            CancellationToken ct = default);

        IAsyncEnumerable<(string Stream, Envelope<IEvent> Event)> ReadEventsAsync(IStreamNameResolver streamNameResolver, IEventDataFormatter formatter,
            CancellationToken ct = default);
    }
}
