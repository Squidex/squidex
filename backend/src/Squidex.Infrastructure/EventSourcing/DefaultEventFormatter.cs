// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class DefaultEventFormatter : IEventFormatter
{
    private readonly IJsonSerializer serializer;
    private readonly TypeRegistry typeRegistry;

    public DefaultEventFormatter(TypeRegistry typeRegistry, IJsonSerializer serializer)
    {
        this.typeRegistry = typeRegistry;
        this.serializer = serializer;
    }

    public Envelope<IEvent>? ParseIfKnown(StoredEvent storedEvent)
    {
        return ParseCore(storedEvent);
    }

    public Envelope<IEvent> Parse(StoredEvent storedEvent)
    {
        var envelope = ParseCore(storedEvent);

        if (envelope == null)
        {
            ThrowHelper.InvalidOperationException($"Cannot find event with type name '{storedEvent.Data.Type}'.");
            return default!;
        }

        return envelope;
    }

    private Envelope<IEvent>? ParseCore(StoredEvent storedEvent)
    {
        Guard.NotNull(storedEvent);

        if (!typeRegistry.TryGetType<IEvent>(storedEvent.Data.Type, out var type))
        {
            return null;
        }

        var payload = serializer.Deserialize<IEvent>(storedEvent.Data.Payload, type);

        if (payload is IMigrated<IEvent> migratedEvent)
        {
            payload = migratedEvent.Migrate();
        }

        var envelope = new Envelope<IEvent>(payload, storedEvent.Data.Headers);

        envelope.SetEventPosition(storedEvent.EventPosition);
        envelope.SetEventStreamNumber(storedEvent.EventStreamNumber);

        return envelope;
    }

    public EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true)
    {
        var payload = envelope.Payload;

        if (migrate && payload is IMigrated<IEvent> migratedEvent)
        {
            payload = migratedEvent.Migrate();
        }

        var payloadType = typeRegistry.GetName<IEvent>(payload.GetType());
        var payloadJson = serializer.Serialize(envelope.Payload, envelope.Payload.GetType());

        envelope.SetCommitId(commitId);

        return new EventData(payloadType, envelope.Headers, payloadJson);
    }
}
