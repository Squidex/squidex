﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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

        public sealed record TestAction : RuleAction
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
        public async Task Should_not_insert_job_if_null()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var rule = CreateRule();

            var job = new RuleJob
            {
                Created = now
            };

            A.CallTo(() => ruleService.CreateJobsAsync(@event, A<RuleContext>.That.Matches(x => x.Rule == rule.RuleDef), default))
                .Returns(new List<JobResult> { new JobResult(null) }.ToAsyncEnumerable());

            await sut.EnqueueAsync(rule.RuleDef, rule.Id, @event);

            A.CallTo(() => ruleEventRepository.EnqueueAsync(A<RuleJob>._, (Exception?)null))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_update_repository_if_enqueing()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var rule = CreateRule();

            var job = new RuleJob
            {
                Created = now
            };

            A.CallTo(() => ruleService.CreateJobsAsync(@event, A<RuleContext>.That.Matches(x => x.Rule == rule.RuleDef), default))
                .Returns(new List<JobResult> { new JobResult(job) }.ToAsyncEnumerable());

            await sut.EnqueueAsync(rule.RuleDef, rule.Id, @event);

            A.CallTo(() => ruleEventRepository.EnqueueAsync(job, (Exception?)null))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_update_repository_with_jobs_from_service()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var job1 = new RuleJob
            {
                Created = now
            };

            SetupRules(@event, job1);

            await sut.On(@event);

            A.CallTo(() => ruleEventRepository.EnqueueAsync(job1, (Exception?)null))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_eqneue_if_event_restored()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var job1 = new RuleJob { Created = now };

            SetupRules(@event, job1);

            await sut.On(@event.SetRestored(true));

            A.CallTo(() => ruleEventRepository.EnqueueAsync(A<RuleJob>._, A<Exception?>._))
                .MustNotHaveHappened();
        }

        private void SetupRules(Envelope<IEvent> @event, RuleJob job1)
        {
            var rule1 = CreateRule();
            var rule2 = CreateRule();

            A.CallTo(() => appProvider.GetRulesAsync(appId.Id))
                .Returns(new List<IRuleEntity> { rule1, rule2 });

            A.CallTo(() => ruleService.CreateJobsAsync(@event, A<RuleContext>.That.Matches(x => x.Rule == rule1.RuleDef), default))
                .Returns(new List<JobResult> { new JobResult(job1) }.ToAsyncEnumerable());

            A.CallTo(() => ruleService.CreateJobsAsync(@event, A<RuleContext>.That.Matches(x => x.Rule == rule2.RuleDef), default))
                .Returns(new List<JobResult>().ToAsyncEnumerable());
        }

        private static RuleEntity CreateRule()
        {
            var rule = new Rule(new ContentChangedTriggerV2(), new TestAction { Url = new Uri("https://squidex.io") });

            return new RuleEntity { RuleDef = rule, Id = DomainId.NewGuid() };
        }
    }
}
