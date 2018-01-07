// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using NodaTime;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EnvelopeExtensionsTests
    {
        private readonly Envelope<string> sut = new Envelope<string>(string.Empty);
        private readonly CultureInfo culture = CultureInfo.InvariantCulture;

        [Fact]
        public void Should_set_and_get_timestamp()
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();

            sut.SetTimestamp(timestamp);

            Assert.Equal(timestamp, sut.Headers.Timestamp());
            Assert.Equal(timestamp, sut.Headers["Timestamp"].ToInstant(culture));
        }

        [Fact]
        public void Should_set_and_get_commit_id()
        {
            var commitId = Guid.NewGuid();

            sut.SetCommitId(commitId);

            Assert.Equal(commitId, sut.Headers.CommitId());
            Assert.Equal(commitId, sut.Headers["CommitId"].ToGuid(culture));
        }

        [Fact]
        public void Should_set_and_get_event_id()
        {
            var commitId = Guid.NewGuid();

            sut.SetEventId(commitId);

            Assert.Equal(commitId, sut.Headers.EventId());
            Assert.Equal(commitId, sut.Headers["EventId"].ToGuid(culture));
        }

        [Fact]
        public void Should_set_and_get_aggregate_id()
        {
            var commitId = Guid.NewGuid();

            sut.SetAggregateId(commitId);

            Assert.Equal(commitId, sut.Headers.AggregateId());
            Assert.Equal(commitId, sut.Headers["AggregateId"].ToGuid(culture));
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
            Assert.Equal(eventStreamNumber, sut.Headers["EventStreamNumber"].ToInt64(culture));
        }
    }
}
