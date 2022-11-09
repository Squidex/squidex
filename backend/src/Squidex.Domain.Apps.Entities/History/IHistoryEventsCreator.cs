// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.History;

public interface IHistoryEventsCreator
{
    IReadOnlyDictionary<string, string> Texts { get; }

    Task<HistoryEvent?> CreateEventAsync(Envelope<IEvent> @event);
}
