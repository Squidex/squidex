// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupReader : IDisposable
    {
        int ReadAttachments { get; }

        int ReadEvents { get; }

        Task ReadBlobAsync(string name, Func<Stream, Task> handler);

        Task ReadEventsAsync(IStreamNameResolver streamNameResolver, IEventDataFormatter formatter, Func<(string Stream, Envelope<IEvent> Event), Task> handler);

        Task<T> ReadJsonAsync<T>(string name);
    }
}