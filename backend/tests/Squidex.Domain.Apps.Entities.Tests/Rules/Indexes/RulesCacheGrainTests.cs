// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Core;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public class RulesCacheGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly IRuleRepository ruleRepository = A.Fake<IRuleRepository>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly RulesCacheGrain sut;

        public RulesCacheGrainTests()
        {
            A.CallTo(() => identity.PrimaryKeyString)
                .Returns(appId.ToString());

            sut = new RulesCacheGrain(identity, ruleRepository);
        }

        [Fact]
        public async Task Should_provide_rule_ids_from_repository_once()
        {
            var ids = new List<DomainId>
            {
                DomainId.NewGuid(),
                DomainId.NewGuid()
            };

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            var result1 = await sut.GetRuleIdsAsync();
            var result2 = await sut.GetRuleIdsAsync();

            Assert.Equal(ids, result1);
            Assert.Equal(ids, result2);

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_add_id_to_loaded_result()
        {
            var ids = new List<DomainId>
            {
                DomainId.NewGuid(),
                DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            await sut.GetRuleIdsAsync();
            await sut.AddAsync(newId);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(ids.Union(Enumerable.Repeat(newId, 1)), result);

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_remove_id_from_loaded_result()
        {
            var ids = new List<DomainId>
            {
                DomainId.NewGuid(),
                DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            await sut.GetRuleIdsAsync();
            await sut.RemoveAsync(ids[1]);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(ids.Take(1), result);

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_remove_id_from_not_loaded_result()
        {
            var ids = new List<DomainId>
            {
                DomainId.NewGuid(),
                DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .Returns(ids);

            await sut.RemoveAsync(ids.ElementAt(0));

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(ids.Skip(1), result);

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_merge_found_value_with_added_id()
        {
            var foundId = DomainId.NewGuid();

            async Task<List<DomainId>> GetIds()
            {
                await sut.AddAsync(foundId);

                return new List<DomainId>();
            }

            A.CallTo(() => ruleRepository.QueryIdsAsync(appId, default))
                .ReturnsLazily(() => GetIds());

            var result1 = await sut.GetRuleIdsAsync();
            var result2 = await sut.GetRuleIdsAsync();

            Assert.Equal(foundId, result1.Single());
            Assert.Equal(foundId, result2.Single());
        }
    }
}
