// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Orleans.Core;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public class AppsCacheGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly IAppRepository appRepository = A.Fake<IAppRepository>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly AppsCacheGrain sut;

        public AppsCacheGrainTests()
        {
            A.CallTo(() => identity.PrimaryKeyString)
                .Returns(appId.ToString());

            sut = new AppsCacheGrain(identity, appRepository);
        }

        [Fact]
        public async Task Should_not_reserve_name_if_already_used()
        {
            var ids1 = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid()
            };

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1"), default))
                .Returns(ids1);

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name2"), default))
                .Returns(new Dictionary<string, DomainId>());

            var token1 = await sut.ReserveAsync(DomainId.NewGuid(), "name1");
            var token2 = await sut.ReserveAsync(DomainId.NewGuid(), "name2");

            Assert.Null(token1);
            Assert.NotNull(token2);
        }

        [Fact]
        public async Task Should_provide_app_ids_from_repository_once()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2"), default))
                .Returns(ids);

            var result1 = await sut.GetAppIdsAsync(new[] { "name1", "name2" });
            var result2 = await sut.GetAppIdsAsync(new[] { "name1", "name2" });

            Assert.Equal(ids.Values, result1);
            Assert.Equal(ids.Values, result2);

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2"), default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_load_pending_names()
        {
            var ids1 = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            var ids2 = new Dictionary<string, DomainId>
            {
                ["name4"] = DomainId.NewGuid()
            };

            // Name3 has not been found yet, but will not loaded again.
            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2", "name3"), default))
                .Returns(ids1);

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name4"), default))
                .Returns(ids2);

            var result1 = await sut.GetAppIdsAsync(new[] { "name1", "name2", "name3" });
            var result2 = await sut.GetAppIdsAsync(new[] { "name3", "name4" });

            Assert.Equal(ids1.Values, result1);
            Assert.Equal(ids2.Values, result2);
        }

        [Fact]
        public async Task Should_add_id_to_loaded_result()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2"), default))
                .Returns(ids);

            await sut.GetAppIdsAsync(new[] { "name1", "name2" });
            await sut.AddAsync(newId, "new-name");

            var result = await sut.GetAppIdsAsync(new[] { "new-name" });

            Assert.Equal(Enumerable.Repeat(newId, 1), result);

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2"), default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_remove_id_from_loaded_result()
        {
            var ids = new Dictionary<string, DomainId>
            {
                ["name1"] = DomainId.NewGuid(),
                ["name2"] = DomainId.NewGuid()
            };

            var newId = DomainId.NewGuid();

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2"), default))
                .Returns(ids);

            await sut.GetAppIdsAsync(new[] { "name1", "name2" });
            await sut.RemoveAsync(ids.Values.ElementAt(1));

            var result = await sut.GetAppIdsAsync(new[] { "name1", "name2" });

            Assert.Equal(ids.Values.Take(1), result);

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>.That.Is("name1", "name2"), default))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_merge_found_value_with_added_id()
        {
            var foundId = DomainId.NewGuid();

            async Task<Dictionary<string, DomainId>> GetIds()
            {
                await sut.AddAsync(foundId, "name1");

                return new Dictionary<string, DomainId>();
            }

            A.CallTo(() => appRepository.QueryIdsAsync(A<IEnumerable<string>>._, A<CancellationToken>._))
                .ReturnsLazily(() => GetIds());

            var result1 = await sut.GetAppIdsAsync(new[] { "name1" });
            var result2 = await sut.GetAppIdsAsync(new[] { "name1" });

            Assert.Equal(foundId, result1.Single());
            Assert.Equal(foundId, result2.Single());
        }
    }
}
