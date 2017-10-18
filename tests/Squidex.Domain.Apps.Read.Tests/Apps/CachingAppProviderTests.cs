// ==========================================================================
//  CachingAppProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

namespace Squidex.Domain.Apps.Read.Apps
{
    public class CachingAppProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IAppRepository repository = A.Fake<IAppRepository>();
        private readonly CachingAppProvider sut;
        private readonly IAppEntity appV1 = A.Dummy<IAppEntity>();
        private readonly IAppEntity appV2 = A.Dummy<IAppEntity>();
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public CachingAppProviderTests()
        {
            A.CallTo(() => appV1.Id).Returns(appId.Id);
            A.CallTo(() => appV1.Name).Returns(appId.Name);

            A.CallTo(() => appV2.Id).Returns(appId.Id);
            A.CallTo(() => appV2.Name).Returns(appId.Name);

            sut = new CachingAppProvider(cache, repository);
        }

        [Fact]
        public void Should_return_empty_for_events_filter()
        {
            Assert.Equal(string.Empty, sut.EventsFilter);
        }

        [Fact]
        public void Should_return_empty_for_name()
        {
            Assert.Equal(typeof(CachingAppProvider).Name, sut.Name);
        }

        [Fact]
        public void Should_do_nothing_when_clearing()
        {
            Assert.NotNull(sut.ClearAsync());
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_name_if_retrieved_by_id_before()
        {
            A.CallTo(() => repository.FindAppAsync(appId.Id))
                .Returns(appV1);

            await ProvideAppByIdAsync(appV1);
            await ProvideAppByNameAsync(appV1);

            A.CallTo(() => repository.FindAppAsync(appId.Id)).MustHaveHappened();
            A.CallTo(() => repository.FindAppAsync(appId.Name)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_id_if_retrieved_by_name_before()
        {
            A.CallTo(() => repository.FindAppAsync(appId.Name))
                .Returns(appV1);

            await ProvideAppByNameAsync(appV1);
            await ProvideAppByIdAsync(appV1);

            A.CallTo(() => repository.FindAppAsync(appId.Id)).MustNotHaveHappened();
            A.CallTo(() => repository.FindAppAsync(appId.Name)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_clear_cache_for_id_after_update_event()
        {
            A.CallTo(() => repository.FindAppAsync(appId.Id))
                .Returns(appV2);
            A.CallTo(() => repository.FindAppAsync(appId.Id))
                .Returns(appV1).Once();

            await ProvideAppByIdAsync(appV1);

            sut.On(Envelope.Create(new AppLanguageAdded { AppId = appId }).To<IEvent>()).Wait();

            await ProvideAppByIdAsync(appV2);

            A.CallTo(() => repository.FindAppAsync(appId.Id)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Fact]
        public async Task Should_clear_cache_for_name_after_update_event()
        {
            A.CallTo(() => repository.FindAppAsync(appId.Name))
                .Returns(appV2);
            A.CallTo(() => repository.FindAppAsync(appId.Name))
                .Returns(appV1).Once();

            await ProvideAppByNameAsync(appV1);

            sut.On(Envelope.Create(new AppLanguageAdded { AppId = appId }).To<IEvent>()).Wait();

            await ProvideAppByNameAsync(appV2);

            A.CallTo(() => repository.FindAppAsync(appId.Name)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        private async Task ProvideAppByIdAsync(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByIdAsync(appId.Id));
        }

        private async Task ProvideAppByNameAsync(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByNameAsync(appId.Name));
        }
    }
}
