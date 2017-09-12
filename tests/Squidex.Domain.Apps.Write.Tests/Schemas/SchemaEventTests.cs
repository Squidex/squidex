// ==========================================================================
//  SchemaEventTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Events.Schemas.Old;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Squidex.Domain.Apps.Write.Schemas
{
    public class SchemaEventTests
    {
        [Fact]
        public void Should_migrate_webhook_added_event_to_noop()
        {
            var source = new WebhookAdded();

            source.Migrate().ShouldBeSameEventType(new NoopEvent());
        }

        [Fact]
        public void Should_migrate_webhook_deleted_event_to_noop()
        {
            var source = new WebhookDeleted();

            source.Migrate().ShouldBeSameEventType(new NoopEvent());
        }
    }
}
