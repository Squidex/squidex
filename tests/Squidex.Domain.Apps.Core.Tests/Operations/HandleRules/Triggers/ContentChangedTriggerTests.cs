// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Core.Operations.HandleRules.Triggers
{
    public class ContentChangedTriggerTests
    {
        private readonly IRuleTriggerHandler sut = new ContentChangedTriggerHandler();
        private static readonly NamedId<Guid> SchemaMatch = NamedId.Of(Guid.NewGuid(), "my-schema1");
        private static readonly NamedId<Guid> SchemaNonMatch = NamedId.Of(Guid.NewGuid(), "my-schema2");

        public static IEnumerable<object[]> TestData = new[]
        {
            new object[] { 0, 1, 1, 1, 1, new RuleCreated() },
            new object[] { 0, 1, 1, 1, 1, new ContentCreated { SchemaId = SchemaNonMatch } },
            new object[] { 1, 1, 0, 0, 0, new ContentCreated { SchemaId = SchemaMatch } },
            new object[] { 0, 0, 0, 0, 0, new ContentCreated { SchemaId = SchemaMatch } },
            new object[] { 1, 0, 1, 0, 0, new ContentUpdated { SchemaId = SchemaMatch } },
            new object[] { 0, 0, 0, 0, 0, new ContentUpdated { SchemaId = SchemaMatch } },
            new object[] { 1, 0, 0, 1, 0, new ContentDeleted { SchemaId = SchemaMatch } },
            new object[] { 0, 0, 0, 0, 0, new ContentDeleted { SchemaId = SchemaMatch } },
            new object[] { 1, 0, 0, 0, 1, new ContentStatusChanged { SchemaId = SchemaMatch, Status = Status.Published } },
            new object[] { 0, 0, 0, 0, 0, new ContentStatusChanged { SchemaId = SchemaMatch, Status = Status.Published } },
            new object[] { 0, 1, 1, 1, 1, new ContentStatusChanged { SchemaId = SchemaMatch, Status = Status.Archived } },
            new object[] { 0, 1, 1, 1, 1, new ContentStatusChanged { SchemaId = SchemaMatch, Status = Status.Draft } },
            new object[] { 0, 1, 1, 1, 1, new SchemaCreated { SchemaId = SchemaNonMatch } }
        };

        [Fact]
        public void Should_return_false_when_trigger_contains_no_schemas()
        {
            var trigger = new ContentChangedTrigger();

            var result = sut.Triggers(new Envelope<AppEvent>(new ContentCreated()), trigger);

            Assert.False(result);
        }

        [Fact]
        public void Should_return_true_when_cathing_all_events()
        {
            var trigger = new ContentChangedTrigger { HandleAll = true };

            var result = sut.Triggers(new Envelope<AppEvent>(new ContentCreated()), trigger);

            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_return_result_depending_on_event(int expected, int sendCreate, int sendUpdate, int sendDelete, int sendPublish, AppEvent @event)
        {
            var trigger = new ContentChangedTrigger
            {
                Schemas = ImmutableList.Create(
                    new ContentChangedTriggerSchema
                    {
                        SendCreate = sendCreate == 1,
                        SendUpdate = sendUpdate == 1,
                        SendDelete = sendDelete == 1,
                        SendPublish = sendPublish == 1,
                        SchemaId = SchemaMatch.Id
                    })
            };

            var result = sut.Triggers(new Envelope<AppEvent>(@event), trigger);

            Assert.Equal(expected == 1, result);
        }
    }
}
