// ==========================================================================
//  CachingAppProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Xunit;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local

namespace Squidex.Domain.Apps.Read.Apps
{
    public class CachingAppProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Mock<IAppRepository> repository = new Mock<IAppRepository>();
        private readonly CachingAppProvider sut;
        private readonly IAppEntity appV1;
        private readonly IAppEntity appV2;
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public CachingAppProviderTests()
        {
            var appV1Mock = new Mock<IAppEntity>();
            var appV2Mock = new Mock<IAppEntity>();

            appV1Mock.Setup(x => x.Id).Returns(appId.Id);
            appV1Mock.Setup(x => x.Name).Returns(appId.Name);

            appV2Mock.Setup(x => x.Id).Returns(appId.Id);
            appV2Mock.Setup(x => x.Name).Returns(appId.Name);

            appV1 = appV1Mock.Object;
            appV2 = appV2Mock.Object;

            sut = new CachingAppProvider(cache, repository.Object);
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
            repository.Setup(x => x.FindAppAsync(appId.Id)).Returns(Task.FromResult(appV1));

            await ProvideAppById(appV1);
            await ProvideAppByName(appV1);

            repository.Verify(x => x.FindAppAsync(appId.Id), Times.Once());
            repository.Verify(x => x.FindAppAsync(appId.Name), Times.Never());
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_id_if_retrieved_by_name_before()
        {
            repository.Setup(x => x.FindAppAsync(appId.Name)).Returns(Task.FromResult(appV1));

            await ProvideAppByName(appV1);
            await ProvideAppById(appV1);

            repository.Verify(x => x.FindAppAsync(appId.Name), Times.Once());
            repository.Verify(x => x.FindAppAsync(appId.Id), Times.Never());
        }

        [Fact]
        public async Task Should_clear_cache_for_id_after_update_event()
        {
            var apps = ProviderResults(appV1, appV2);

            repository.Setup(x => x.FindAppAsync(appId.Id)).Returns(() => Task.FromResult(apps()));

            await ProvideAppById(appV1);

            sut.On(Envelope.Create(new AppLanguageAdded {AppId = appId }).To<IEvent>()).Wait();

            await ProvideAppById(appV2);

            repository.Verify(x => x.FindAppAsync(appId.Id), Times.Exactly(2));
        }
        
        [Fact]
        public async Task Should_clear_cache_for_name_after_update_event()
        {
            var apps = ProviderResults(appV1, appV2);

            repository.Setup(x => x.FindAppAsync(appId.Name)).Returns(() => Task.FromResult(apps()));

            await ProvideAppByName(appV1);

            sut.On(Envelope.Create(new AppLanguageAdded { AppId = appId }).To<IEvent>()).Wait();

            await ProvideAppByName(appV2);

            repository.Verify(x => x.FindAppAsync(appId.Name), Times.Exactly(2));
        }

        private async Task ProvideAppById(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByIdAsync(appId.Id));
        }

        private async Task ProvideAppByName(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByNameAsync(appId.Name));
        }

        private static Func<T> ProviderResults<T>(params T[] items)
        {
            var index = 0;

            return () => items[index++];
        }
    }
}
