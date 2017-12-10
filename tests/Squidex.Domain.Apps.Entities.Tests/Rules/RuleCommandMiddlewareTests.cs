// ==========================================================================
//  RuleCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleCommandMiddlewareTests : HandlerTestBase<RuleDomainObject>
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly RuleDomainObject rule = new RuleDomainObject();
        private readonly RuleTrigger ruleTrigger = new ContentChangedTrigger();
        private readonly RuleAction ruleAction = new WebhookAction { Url = new Uri("https://squidex.io") };
        private readonly Guid ruleId = Guid.NewGuid();
        private readonly RuleCommandMiddleware sut;

        protected override Guid Id
        {
            get { return ruleId; }
        }

        public RuleCommandMiddlewareTests()
        {
            A.CallTo(() => appProvider.GetSchemaAsync(A<Guid>.Ignored, A<Guid>.Ignored, false))
                .Returns(A.Fake<ISchemaEntity>());

            sut = new RuleCommandMiddleware(Handler, appProvider);
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var context = CreateContextForCommand(new CreateRule { Trigger = ruleTrigger, Action = ruleAction });

            await TestCreate(rule, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            var context = CreateContextForCommand(new UpdateRule { Trigger = ruleTrigger, Action = ruleAction });

            CreateRule();

            await TestUpdate(rule, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Enable_should_update_domain_object()
        {
            CreateRule();
            DisableRule();

            var command = CreateContextForCommand(new EnableRule { RuleId = ruleId });

            await TestUpdate(rule, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        [Fact]
        public async Task Disable_should_update_domain_object()
        {
            CreateRule();

            var command = CreateContextForCommand(new DisableRule { RuleId = ruleId });

            await TestUpdate(rule, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateRule();

            var command = CreateContextForCommand(new DeleteRule { RuleId = ruleId });

            await TestUpdate(rule, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        private void DisableRule()
        {
            rule.Disable(CreateCommand(new DisableRule()));
        }

        private void CreateRule()
        {
            rule.Create(CreateCommand(new CreateRule { Trigger = ruleTrigger, Action = ruleAction }));
        }
    }
}