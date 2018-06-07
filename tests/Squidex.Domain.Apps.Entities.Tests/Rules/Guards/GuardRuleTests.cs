// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public class GuardRuleTests
    {
        private readonly Uri validUrl = new Uri("https://squidex.io");
        private readonly Rule rule_0 = new Rule(new ContentChangedTrigger(), new WebhookAction());
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();

        public GuardRuleTests()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_trigger_null()
        {
            var command = CreateCommand(new CreateRule
            {
                Trigger = null,
                Action = new WebhookAction
                {
                    Url = validUrl
                }
            });

            await Assert.ThrowsAsync<ValidationException>(() => GuardRule.CanCreate(command, appProvider));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_action_null()
        {
            var command = CreateCommand(new CreateRule
            {
                Trigger = new ContentChangedTrigger
                {
                    Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
                },
                Action = null
            });

            await Assert.ThrowsAsync<ValidationException>(() => GuardRule.CanCreate(command, appProvider));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_trigger_and_action_valid()
        {
            var command = CreateCommand(new CreateRule
            {
                Trigger = new ContentChangedTrigger
                {
                    Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
                },
                Action = new WebhookAction
                {
                    Url = validUrl
                }
            });

            await GuardRule.CanCreate(command, appProvider);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_action_and_trigger_are_null()
        {
            var command = new UpdateRule();

            await Assert.ThrowsAsync<ValidationException>(() => GuardRule.CanUpdate(command, appId.Id, appProvider));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_trigger_and_action_valid()
        {
            var command = new UpdateRule
            {
                Trigger = new ContentChangedTrigger
                {
                    Schemas = ImmutableList<ContentChangedTriggerSchema>.Empty
                },
                Action = new WebhookAction
                {
                    Url = validUrl
                }
            };

            await GuardRule.CanUpdate(command, appId.Id, appProvider);
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_rule_enabled()
        {
            var command = new EnableRule();

            var rule_1 = rule_0.Enable();

            Assert.Throws<ValidationException>(() => GuardRule.CanEnable(command, rule_1));
        }

        [Fact]
        public void CanEnable_should_not_throw_exception_if_rule_disabled()
        {
            var command = new EnableRule();

            var rule_1 = rule_0.Disable();

            GuardRule.CanEnable(command, rule_1);
        }

        [Fact]
        public void CanDisable_should_throw_exception_if_rule_disabled()
        {
            var command = new DisableRule();

            var rule_1 = rule_0.Disable();

            Assert.Throws<ValidationException>(() => GuardRule.CanDisable(command, rule_1));
        }

        [Fact]
        public void CanDisable_should_not_throw_exception_if_rule_enabled()
        {
            var command = new DisableRule();

            var rule_1 = rule_0.Enable();

            GuardRule.CanDisable(command, rule_1);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteRule();

            GuardRule.CanDelete(command);
        }

        private CreateRule CreateCommand(CreateRule command)
        {
            command.AppId = appId;

            return command;
        }
    }
}
