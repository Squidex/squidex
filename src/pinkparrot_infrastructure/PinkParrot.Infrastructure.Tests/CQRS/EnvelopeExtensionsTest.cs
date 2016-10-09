using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using NuGet.Versioning;
using Xunit;

namespace PinkParrot.Infrastructure.CQRS
{
    public class EnvelopeExtensionsTest
    {
        private readonly Envelope<object> envelope = new Envelope<object>(1, new EnvelopeHeaders());

        [Fact]
        public void Should_set_and_get_timestamp()
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();

            envelope.SetTimestamp(timestamp);

            Assert.Equal(timestamp, envelope.Headers.Timestamp());
        }

        [Fact]
        public void Should_set_and_get_event_id()
        {
            var eventId = Guid.NewGuid();

            envelope.SetEventId(eventId);

            Assert.Equal(eventId, envelope.Headers.EventId());
        }

        [Fact]
        public void Should_set_and_get_event_number()
        {
            const int eventNumber = 123;

            envelope.SetEventNumber(eventNumber);

            Assert.Equal(eventNumber, envelope.Headers.EventNumber());
        }

        [Fact]
        public void Should_set_and_get_aggregate_id()
        {
            var aggregateId = Guid.NewGuid();

            envelope.SetAggregateId(aggregateId);

            Assert.Equal(aggregateId, envelope.Headers.AggregateId());
        }

        [Fact]
        public void Should_set_and_get_tenant_id()
        {
            var tenantId = Guid.NewGuid();

            envelope.SetTenantId(tenantId);

            Assert.Equal(tenantId, envelope.Headers.TenantId());
        }

        [Fact]
        public void Should_set_and_get_commit_id()
        {
            var commitId = Guid.NewGuid();

            envelope.SetCommitId(commitId);

            Assert.Equal(commitId, envelope.Headers.CommitId());
        }
    }
}
