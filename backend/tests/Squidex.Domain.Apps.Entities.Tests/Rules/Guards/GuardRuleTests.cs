﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Rules.Guards
{
    public class GuardRuleTests
    {
        private readonly Uri validUrl = new Uri("https://squidex.io");
        private readonly Rule rule_0 = new Rule(new ContentChangedTriggerV2(), new TestAction()).Rename("MyName");
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();

        public sealed class TestAction : RuleAction
        {
            public Uri Url { get; set; }
        }

        public GuardRuleTests()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(Mocks.Schema(appId, schemaId));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_trigger_null()
        {
            var command = CreateCommand(new CreateRule
            {
                Trigger = null!,
                Action = new TestAction
                {
                    Url = validUrl
                }
            });

            await ValidationAssert.ThrowsAsync(() => GuardRule.CanCreate(command, appProvider),
                new ValidationError("Trigger is required.", "Trigger"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_action_null()
        {
            var command = CreateCommand(new CreateRule
            {
                Trigger = new ContentChangedTriggerV2
                {
                    Schemas = ReadOnlyCollection.Empty<ContentChangedTriggerSchemaV2>()
                },
                Action = null!
            });

            await ValidationAssert.ThrowsAsync(() => GuardRule.CanCreate(command, appProvider),
                new ValidationError("Action is required.", "Action"));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_trigger_and_action_valid()
        {
            var command = CreateCommand(new CreateRule
            {
                Trigger = new ContentChangedTriggerV2
                {
                    Schemas = ReadOnlyCollection.Empty<ContentChangedTriggerSchemaV2>()
                },
                Action = new TestAction
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

            await ValidationAssert.ThrowsAsync(() => GuardRule.CanUpdate(command, appId.Id, appProvider, rule_0),
                new ValidationError("Either trigger, action or name is required.", "Trigger", "Action"));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_rule_has_already_this_name()
        {
            var command = new UpdateRule { Name = "MyName" };

            await GuardRule.CanUpdate(command, appId.Id, appProvider, rule_0);
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_trigger_action__and_name_are_valid()
        {
            var command = new UpdateRule
            {
                Trigger = new ContentChangedTriggerV2
                {
                    Schemas = ReadOnlyCollection.Empty<ContentChangedTriggerSchemaV2>()
                },
                Action = new TestAction
                {
                    Url = validUrl
                },
                Name = "NewName"
            };

            await GuardRule.CanUpdate(command, appId.Id, appProvider, rule_0);
        }

        [Fact]
        public void CanEnable_should_throw_exception_if_rule_enabled()
        {
            var command = new EnableRule();

            var rule_1 = rule_0.Enable();

            Assert.Throws<DomainException>(() => GuardRule.CanEnable(command, rule_1));
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

            Assert.Throws<DomainException>(() => GuardRule.CanDisable(command, rule_1));
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
