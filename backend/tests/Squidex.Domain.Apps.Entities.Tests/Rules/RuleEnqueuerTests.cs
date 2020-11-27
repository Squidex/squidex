// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleEnqueuerTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly ILocalCache localCache = A.Fake<ILocalCache>();
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly IRuleService ruleService = A.Fake<IRuleService>();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly RuleEnqueuer sut;

        public sealed class TestAction : RuleAction
        {
            public Uri Url { get; set; }
        }

        public RuleEnqueuerTests()
        {
            sut = new RuleEnqueuer(
                appProvider,
                cache,
                localCache,
                ruleEventRepository,
                ruleService);
        }

        [Fact]
        public void Should_return_wildcard_filter_for_events_filter()
        {
            IEventConsumer consumer = sut;

            Assert.Equal(".*", consumer.EventsFilter);
        }

        [Fact]
        public async Task Should_do_nothing_on_clear()
        {
            IEventConsumer consumer = sut;

            await consumer.ClearAsync();
        }

        [Fact]
        public void Should_return_type_name_for_name()
        {
            IEventConsumer consumer = sut;

            Assert.Equal(nameof(RuleEnqueuer), consumer.Name);
        }

        [Fact]
        public async Task Should_update_repository_when_enqueing()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var rule = CreateRule();

            var job = new RuleJob { Created = now };

            A.CallTo(() => ruleService.CreateJobsAsync(rule.RuleDef, rule.Id, @event, true))
                .Returns(new List<(RuleJob, Exception?)> { (job, null) });

            await sut.EnqueueAsync(rule.RuleDef, rule.Id, @event);

            A.CallTo(() => ruleEventRepository.EnqueueAsync(job, (Exception?)null))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_update_repositories_with_jobs_from_service()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var rule1 = CreateRule();
            var rule2 = CreateRule();

            var job1 = new RuleJob { Created = now };

            A.CallTo(() => appProvider.GetRulesAsync(appId.Id))
                .Returns(new List<IRuleEntity> { rule1, rule2 });

            A.CallTo(() => ruleService.CreateJobsAsync(rule1.RuleDef, rule1.Id, @event, true))
                .Returns(new List<(RuleJob, Exception?)> { (job1, null) });

            A.CallTo(() => ruleService.CreateJobsAsync(rule2.RuleDef, rule2.Id, @event, true))
                .Returns(new List<(RuleJob, Exception?)>());

            await sut.On(@event);

            A.CallTo(() => ruleEventRepository.EnqueueAsync(job1, (Exception?)null))
                .MustHaveHappened();
        }

        private static RuleEntity CreateRule()
        {
            var rule = new Rule(new ContentChangedTriggerV2(), new TestAction { Url = new Uri("https://squidex.io") });

            return new RuleEntity { RuleDef = rule, Id = DomainId.NewGuid() };
        }
    }
}