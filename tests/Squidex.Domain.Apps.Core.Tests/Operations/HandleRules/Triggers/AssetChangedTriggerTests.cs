// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules.Triggers
{
    public class AssetChangedTriggerTests
    {
        private readonly IRuleTriggerHandler sut = new AssetChangedTriggerHandler();

        public static IEnumerable<object[]> TestData
        {
            get
            {
                return new[]
                {
                    new object[] { 0, 1, 1, 1, 1, new RuleCreated() },
                    new object[] { 1, 1, 0, 0, 0, new AssetCreated() },
                    new object[] { 0, 0, 0, 0, 0, new AssetCreated() },
                    new object[] { 1, 0, 1, 0, 0, new AssetUpdated() },
                    new object[] { 0, 0, 0, 0, 0, new AssetUpdated() },
                    new object[] { 1, 0, 0, 1, 0, new AssetRenamed() },
                    new object[] { 0, 0, 0, 0, 0, new AssetRenamed() },
                    new object[] { 1, 0, 0, 0, 1, new AssetDeleted() },
                    new object[] { 0, 0, 0, 0, 0, new AssetDeleted() }
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Should_return_result_depending_on_event(int expected, int sendCreate, int sendUpdate, int sendRename, int sendDelete, AppEvent @event)
        {
            var trigger = new AssetChangedTrigger
            {
                SendCreate = sendCreate == 1,
                SendUpdate = sendUpdate == 1,
                SendRename = sendRename == 1,
                SendDelete = sendDelete == 1
            };

            var result = sut.Triggers(new Envelope<AppEvent>(@event), trigger);

            Assert.Equal(expected == 1, result);
        }
    }
}
