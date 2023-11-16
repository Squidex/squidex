// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using EventStore.Client;
using NodaTime;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class GetEventStore : IEventStore, IInitializable
{
    private const string StreamPrefix = "squidex";
    private readonly EventStoreClient client;
    private readonly EventStoreProjectionClient projectionClient;
    private readonly IJsonSerializer serializer;

    public GetEventStore(EventStoreClientSettings settings, IJsonSerializer serializer)
    {
        this.serializer = serializer;

        client = new EventStoreClient(settings);

        projectionClient = new EventStoreProjectionClient(settings, StreamPrefix);
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        try
        {
            await client.DeleteAsync(Guid.NewGuid().ToString(), StreamState.Any, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            var error = new ConfigurationError("GetEventStore cannot connect to event store.");

            throw new ConfigurationException(error, ex);
        }
    }

    public IEventSubscription CreateSubscription(IEventSubscriber<StoredEvent> subscriber, StreamFilter filter, string? position = null)
    {
        Guard.NotNull(filter);

        return new GetEventStoreSubscription(subscriber, client, projectionClient, serializer, position, StreamPrefix, filter);
    }

    public async IAsyncEnumerable<StoredEvent> QueryAllAsync(StreamFilter filter, string? position = null, int take = int.MaxValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (take <= 0)
        {
            yield break;
        }

        var streamName = await projectionClient.CreateProjectionAsync(filter);

        var stream = QueryAsync(streamName, position.ToPosition(false), take, ct);

        await foreach (var storedEvent in stream.IgnoreNotFound(ct))
        {
            yield return storedEvent;
        }
    }

    public async IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(StreamFilter filter, Instant timestamp = default, int take = int.MaxValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (take <= 0)
        {
            yield break;
        }

        var streamName = await projectionClient.CreateProjectionAsync(filter);

        var stream = QueryReverseAsync(streamName, StreamPosition.End, take, ct);

        await foreach (var storedEvent in stream.IgnoreNotFound(ct).TakeWhile(x => x.Data.Headers.Timestamp() >= timestamp).WithCancellation(ct))
        {
            yield return storedEvent;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> QueryStreamAsync(string streamName, long afterStreamPosition = EtagVersion.Empty,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("GetEventStore/QueryAsync"))
        {
            var result = new List<StoredEvent>();

            var stream = QueryAsync(streamName, afterStreamPosition.ToPositionBefore(), int.MaxValue, ct);

            await foreach (var storedEvent in stream.IgnoreNotFound(ct))
            {
                result.Add(storedEvent);
            }

            return result.ToList();
        }
    }

    private IAsyncEnumerable<StoredEvent> QueryAsync(string streamName, StreamPosition start, long count,
        CancellationToken ct = default)
    {
        var result = client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            start,
            count,
            true,
            cancellationToken: ct);

        return result.Select(x => Formatter.Read(x, StreamPrefix, serializer));
    }

    private IAsyncEnumerable<StoredEvent> QueryReverseAsync(string streamName, StreamPosition start, long count,
        CancellationToken ct = default)
    {
        var result = client.ReadStreamAsync(
            Direction.Backwards,
            streamName,
            start,
            count,
            true,
            cancellationToken: ct);

        return result.Select(x => Formatter.Read(x, StreamPrefix, serializer));
    }

    public async Task DeleteStreamAsync(string streamName,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(streamName);

        await client.DeleteAsync(GetStreamName(streamName), StreamState.Any, cancellationToken: ct);
    }

    public async Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(streamName);
        Guard.NotNull(events);
        Guard.GreaterEquals(expectedVersion, EtagVersion.Any);

        using (Telemetry.Activities.StartActivity("GetEventStore/AppendEventsInternalAsync"))
        {
            if (events.Count == 0)
            {
                return;
            }

            try
            {
                var eventData = events.Select(x => Formatter.Write(x, serializer));

                streamName = GetStreamName(streamName);

                if (expectedVersion == -1)
                {
                    await client.AppendToStreamAsync(streamName, StreamState.NoStream, eventData, cancellationToken: ct);
                }
                else if (expectedVersion < -1)
                {
                    await client.AppendToStreamAsync(streamName, StreamState.Any, eventData, cancellationToken: ct);
                }
                else
                {
                    await client.AppendToStreamAsync(streamName, expectedVersion.ToRevision(), eventData, cancellationToken: ct);
                }
            }
            catch (WrongExpectedVersionException ex)
            {
                throw new WrongEventVersionException(ex.ActualVersion ?? 0, expectedVersion);
            }
        }
    }

    public async Task DeleteAsync(StreamFilter filter,
        CancellationToken ct = default)
    {
        var streamName = await projectionClient.CreateProjectionAsync(filter);

        var events = client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, resolveLinkTos: true, cancellationToken: ct);

        if (await events.ReadState == ReadState.StreamNotFound)
        {
            return;
        }

        var deleted = new HashSet<string>();

        await foreach (var storedEvent in TaskAsyncEnumerableExtensions.WithCancellation(events, ct))
        {
            var streamToDelete = storedEvent.Event.EventStreamId;

            if (deleted.Add(streamToDelete))
            {
                await client.DeleteAsync(streamToDelete, StreamState.Any, cancellationToken: ct);
            }
        }
    }

    private static string GetStreamName(string streamName)
    {
        return $"{StreamPrefix}-{streamName}";
    }
}
