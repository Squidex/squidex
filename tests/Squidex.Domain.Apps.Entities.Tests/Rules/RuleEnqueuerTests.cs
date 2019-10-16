﻿// ==========================================================================
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
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly RuleService ruleService = A.Fake<RuleService>();
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
                ruleEventRepository,
                ruleService);
        }

        [Fact]
        public void Should_return_contents_filter_for_events_filter()
        {
            Assert.Equal(".*", sut.EventsFilter);
        }

        [Fact]
        public void Should_return_type_name_for_name()
        {
            Assert.Equal(typeof(RuleEnqueuer).Name, sut.Name);
        }

        [Fact]
        public async Task Should_do_nothing_on_clear()
        {
            await sut.ClearAsync();
        }

        [Fact]
        public async Task Should_update_repositories_on_with_jobs_from_sender()
        {
            var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = appId });

            var rule1 = new Rule(new ContentChangedTriggerV2(), new TestAction { Url = new Uri("https://squidex.io") });
            var rule2 = new Rule(new ContentChangedTriggerV2(), new TestAction { Url = new Uri("https://squidex.io") });

            var job1 = new RuleJob { Created = now };

            var ruleEntity1 = new RuleEntity { RuleDef = rule1 };
            var ruleEntity2 = new RuleEntity { RuleDef = rule2 };

            A.CallTo(() => appProvider.GetRulesAsync(appId.Id))
                .Returns(new List<IRuleEntity> { ruleEntity1, ruleEntity2 });

            A.CallTo(() => ruleService.CreateJobAsync(rule1, ruleEntity1.Id, @event))
                .Returns(job1);

            A.CallTo(() => ruleService.CreateJobAsync(rule2, ruleEntity2.Id, @event))
                .Returns(Task.FromResult<RuleJob>(null));

            await sut.On(@event);

            A.CallTo(() => ruleEventRepository.EnqueueAsync(job1, now))
                .MustHaveHappened();
        }
    }
}