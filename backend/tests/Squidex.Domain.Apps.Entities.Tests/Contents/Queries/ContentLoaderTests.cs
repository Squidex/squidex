// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentLoaderTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IContentGrain grain = A.Fake<IContentGrain>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId id = DomainId.NewGuid();
        private readonly ContentLoader sut;

        public ContentLoaderTests()
        {
            var key = DomainId.Combine(appId, id).ToString();

            A.CallTo(() => grainFactory.GetGrain<IContentGrain>(key, null))
                .Returns(grain);

            sut = new ContentLoader(grainFactory);
        }

        [Fact]
        public async Task Should_return_null_if_no_state_returned()
        {
            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(null!));

            Assert.Null(await sut.GetAsync(appId, id, 10));
        }

        [Fact]
        public async Task Should_return_null_if_state_empty()
        {
            var content = new ContentEntity { Version = EtagVersion.Empty };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(content));

            Assert.Null(await sut.GetAsync(appId, id, 10));
        }

        [Fact]
        public async Task Should_return_null_if_state_has_other_version()
        {
            var content = new ContentEntity { Version = 5 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(content));

            Assert.Null(await sut.GetAsync(appId, id, 10));
        }

        [Fact]
        public async Task Should_not_return_null_if_state_has_other_version_than_any()
        {
            var content = new ContentEntity { Version = 5 };

            A.CallTo(() => grain.GetStateAsync(EtagVersion.Any))
                .Returns(J.Of<IContentEntity>(content));

            await sut.GetAsync(appId, id, EtagVersion.Any);
        }

        [Fact]
        public async Task Should_return_content_from_state()
        {
            var content = new ContentEntity { Version = 10 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(content));

            var result = await sut.GetAsync(appId, id, 10);

            Assert.Same(content, result);
        }
    }
}
