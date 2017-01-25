// ==========================================================================
//  CachingAppProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Events.Apps;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.MongoDb.Apps;
using Xunit;
// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local

namespace Squidex.Read.Apps
{
    public class CachingAppProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Mock<IAppRepository> repository = new Mock<IAppRepository>();
        private readonly CachingAppProvider sut;
        private readonly MongoAppEntity appV1;
        private readonly MongoAppEntity appV2;
        private readonly Guid appId = Guid.NewGuid();
        private readonly string appName = "my-app";

        private sealed class MyEvent : IEvent
        {
        }

        public CachingAppProviderTests()
        {
            appV1 = new MongoAppEntity { Name = appName, Id = appId };
            appV2 = new MongoAppEntity { Name = appName, Id = appId };

            sut = new CachingAppProvider(cache, repository.Object);
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_name_if_retrieved_by_id_before()
        {
            repository.Setup(x => x.FindAppAsync(appId)).Returns(Task.FromResult<IAppEntity>(appV1));

            await ProvideAppById(appV1);
            await ProvideAppByName(appV1);

            repository.Verify(x => x.FindAppAsync(appId), Times.Once());
            repository.Verify(x => x.FindAppAsync(appName), Times.Never());
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_id_if_retrieved_by_name_before()
        {
            repository.Setup(x => x.FindAppAsync(appName)).Returns(Task.FromResult<IAppEntity>(appV1));

            await ProvideAppByName(appV1);
            await ProvideAppById(appV1);

            repository.Verify(x => x.FindAppAsync(appName), Times.Once());
            repository.Verify(x => x.FindAppAsync(appId), Times.Never());
        }

        [Fact]
        public async Task Should_ignore_other_events()
        {
            repository.Setup(x => x.FindAppAsync(appId)).Returns(Task.FromResult<IAppEntity>(appV1));

            await ProvideAppById(appV1);
            await RaiseEvent(new MyEvent());
            await ProvideAppById(appV1);

            repository.Verify(x => x.FindAppAsync(appId), Times.Once());
        }

        [Fact]
        public async Task Should_retrieve_by_id_after_created_event()
        {
            var apps = ProviderResults(null, appV1);

            repository.Setup(x => x.FindAppAsync(appId)).Returns(() => Task.FromResult<IAppEntity>(apps()));

            await ProvideAppById(null);
            await RaiseEvent(new AppCreated { Name = appName });
            await ProvideAppById(appV1);

            repository.Verify(x => x.FindAppAsync(appId), Times.Exactly(2));
        }

        [Fact]
        public async Task Should_retrieve_by_name_after_created_event()
        {
            var apps = ProviderResults(null, appV1);

            repository.Setup(x => x.FindAppAsync(appName)).Returns(() => Task.FromResult<IAppEntity>(apps()));

            await ProvideAppByName(null);
            await RaiseEvent(new AppCreated { Name = appName });
            await ProvideAppByName(appV1);

            repository.Verify(x => x.FindAppAsync(appName), Times.Exactly(2));
        }

        [Theory]
        [MemberData(nameof(AppEvents))]
        public async Task Should_clear_cache_for_id_after_update_event(IEvent @event)
        {
            var apps = ProviderResults(appV1, appV2);

            repository.Setup(x => x.FindAppAsync(appId)).Returns(() => Task.FromResult<IAppEntity>(apps()));

            await ProvideAppById(appV1);
            await RaiseEvent(@event);
            await ProvideAppById(appV2);

            repository.Verify(x => x.FindAppAsync(appId), Times.Exactly(2));
        }
        
        [Theory]
        [MemberData(nameof(AppEvents))]
        public async Task Should_clear_cache_for_name_after_update_event(IEvent @event)
        {
            var apps = ProviderResults(appV1, appV2);

            repository.Setup(x => x.FindAppAsync(appName)).Returns(() => Task.FromResult<IAppEntity>(apps()));

            await ProvideAppByName(appV1);
            await RaiseEvent(@event);
            await ProvideAppByName(appV2);

            repository.Verify(x => x.FindAppAsync(appName), Times.Exactly(2));
        }

        private async Task RaiseEvent(IEvent @event)
        {
            await sut.On(new Envelope<IEvent>(@event).SetAggregateId(appId));
        }

        private async Task ProvideAppById(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByIdAsync(appId));
        }

        private async Task ProvideAppByName(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByNameAsync(appName));
        }

        private static Func<T> ProviderResults<T>(params T[] items)
        {
            var index = 0;

            return () => items[index++];
        }

        public static IEnumerable<object[]> AppEvents
        {
            get
            {
                yield return new object[] { new AppContributorAssigned() };
                yield return new object[] { new AppContributorRemoved() };
                yield return new object[] { new AppClientAttached() };
                yield return new object[] { new AppClientRevoked() };
                yield return new object[] { new AppClientRenamed() };
                yield return new object[] { new AppLanguageAdded() };
                yield return new object[] { new AppLanguageRemoved() };
                yield return new object[] { new AppMasterLanguageSet() };
            }
        }
    }
}
