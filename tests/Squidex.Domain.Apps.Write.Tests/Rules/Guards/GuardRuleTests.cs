// ==========================================================================
//  GuardRuleTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write.Rules.Commands;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.Rules.Guards
{
    public class GuardRuleTests
    {
        private readonly Uri validUrl = new Uri("https://squidex.io");
        private readonly Rule rule = new Rule(new ContentChangedTrigger(), new WebhookAction());
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();

        public GuardRuleTests()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Name, A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_trigger_null()
        {
            var command = new CreateRule
            {
                Trigger = null,
                Action = new WebhookAction
                {
                    Url = validUrl
                },
                AppId = appId
            };

            await Assert.ThrowsAsync<ValidationException>(() => GuardRule.CanCreate(command, appProvider));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_action_null()
        {
            var command = new CreateRule
            {
                Trigger = new ContentChangedTrigger
                {
                    Schemas = new List<ContentChangedTriggerSchema>()
                },
                Action = null,
                AppId = appId
            };

            await Assert.ThrowsAsync<ValidationException>(() => GuardRule.CanCreate(command, appProvider));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_trigger_and_action_valid()
        {
            var command = new CreateRule
            {
                Trigger = new ContentChangedTrigger
                {
                    Schemas = new List<ContentChangedTriggerSchema>()
                },
                Action = new WebhookAction
                {
                    Url = validUrl
                },
                AppId = appId
            };

            await GuardRule.CanCreate(command, appProvider);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_action_and_trigger_are_null()
        {
            var command = new UpdateRule();

            await Assert.ThrowsAsync<ValidationException>(() => GuardRule.CanUpdate(command, appProvider));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_trigger_and_action_valid()
        {
            var command = new UpdateRule
            {
                Trigger = new ContentChangedTrigger
                {
                    Schemas = new List<ContentChangedTriggerSchema>()
                },
                Action = new WebhookAction
                {
                    Url = validUrl
                },
                AppId = appId
            };

            await GuardRule.CanUpdate(command, appProvider);
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_rule_enabled()
        {
            var command = new EnableRule();

            rule.Enable();

            Assert.Throws<ValidationException>(() => GuardRule.CanEnable(command, rule));
        }

        [Fact]
        public void CanEnable_should_not_throw_exception_if_rule_disabled()
        {
            var command = new EnableRule();

            rule.Disable();

            GuardRule.CanEnable(command, rule);
        }

        [Fact]
        public void CanDisable_should_throw_exception_if_rule_disabled()
        {
            var command = new DisableRule();

            rule.Disable();

            Assert.Throws<ValidationException>(() => GuardRule.CanDisable(command, rule));
        }

        [Fact]
        public void CanDisable_should_not_throw_exception_if_rule_enabled()
        {
            var command = new DisableRule();

            rule.Enable();

            GuardRule.CanDisable(command, rule);
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            var command = new DeleteRule();

            GuardRule.CanDelete(command);
        }
    }
}
