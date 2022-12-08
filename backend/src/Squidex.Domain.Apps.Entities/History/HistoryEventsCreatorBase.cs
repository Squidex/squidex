// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.History;

public abstract class HistoryEventsCreatorBase : IHistoryEventsCreator
{
    private readonly Dictionary<string, string> texts = new Dictionary<string, string>();
    private readonly TypeRegistry typeRegistry;

    public IReadOnlyDictionary<string, string> Texts
    {
        get => texts;
    }

    protected HistoryEventsCreatorBase(TypeRegistry typeRegistry)
    {
        Guard.NotNull(typeRegistry);

        this.typeRegistry = typeRegistry;
    }

    protected void AddEventMessage<TEvent>(string message) where TEvent : IEvent
    {
        Guard.NotNullOrEmpty(message);

        texts[typeRegistry.GetName<IEvent, TEvent>()] = message;
    }

    protected bool HasEventText(IEvent @event)
    {
        var message = typeRegistry.GetName<IEvent>(@event.GetType());

        return texts.ContainsKey(message);
    }

    protected HistoryEvent ForEvent(IEvent @event, string channel)
    {
        var message = typeRegistry.GetName<IEvent>(@event.GetType());

        return new HistoryEvent(channel, message);
    }

    public Task<HistoryEvent?> CreateEventAsync(Envelope<IEvent> @event)
    {
        if (HasEventText(@event.Payload))
        {
            return CreateEventCoreAsync(@event);
        }

        return Task.FromResult<HistoryEvent?>(null);
    }

    protected abstract Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event);
}
