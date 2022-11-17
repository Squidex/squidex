// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.EventSourcing;

public class EnvelopeExtensionsTests
{
    private readonly Envelope<MyEvent> sut = new Envelope<MyEvent>(new MyEvent());

    public sealed class MyEvent : IEvent
    {
    }

    [Fact]
    public void Should_set_and_get_timestamp()
    {
        var timestamp = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

        sut.SetTimestamp(timestamp);

        Assert.Equal(timestamp, sut.Headers.Timestamp());
        Assert.Equal(timestamp, sut.Headers.GetInstant("Timestamp"));
    }

    [Fact]
    public void Should_set_and_get_commit_id()
    {
        var commitId = Guid.NewGuid();

        sut.SetCommitId(commitId);

        Assert.Equal(commitId, sut.Headers.CommitId());
        Assert.Equal(commitId, sut.Headers.GetGuid("CommitId"));
    }

    [Fact]
    public void Should_set_and_get_event_id()
    {
        var commitId = Guid.NewGuid();

        sut.SetEventId(commitId);

        Assert.Equal(commitId, sut.Headers.EventId());
        Assert.Equal(commitId, sut.Headers.GetGuid("EventId"));
    }

    [Fact]
    public void Should_set_and_get_aggregate_id()
    {
        var commitId = DomainId.NewGuid();

        sut.SetAggregateId(commitId);

        Assert.Equal(commitId, sut.Headers.AggregateId());
        Assert.Equal(commitId.ToString(), sut.Headers.GetString("AggregateId"));
    }

    [Fact]
    public void Should_set_and_get_event_number()
    {
        const string eventNumber = "123";

        sut.SetEventPosition(eventNumber);

        Assert.Equal(eventNumber, sut.Headers.EventPosition());
        Assert.Equal(eventNumber, sut.Headers["EventNumber"].ToString());
    }

    [Fact]
    public void Should_set_and_get_event_stream_number()
    {
        const int eventStreamNumber = 123;

        sut.SetEventStreamNumber(eventStreamNumber);

        Assert.Equal(eventStreamNumber, sut.Headers.EventStreamNumber());
        Assert.Equal(eventStreamNumber, sut.Headers.GetLong("EventStreamNumber"));
    }
}
