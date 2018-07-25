// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IRestoreHandler
    {
        string Name { get; }

        Task HandleAsync(Envelope<IEvent> @event, Stream attachment);

        Task ProcessAsync();

        Task CompleteAsync();
    }
}
