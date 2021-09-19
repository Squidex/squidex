// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public class AppsCacheGrainTests
    {
        private readonly IAppRepository appRepository = A.Fake<IAppRepository>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly AppsCacheGrain sut;

        public AppsCacheGrainTests()
        {
            sut = new AppsCacheGrain(appRepository);
            sut.ActivateAsync(appId.ToString()).Wait();
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
    }
}
