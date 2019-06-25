// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentVersionLoaderTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IContentGrain grain = A.Fake<IContentGrain>();
        private readonly Guid id = Guid.NewGuid();
        private readonly ContentVersionLoader sut;

        public ContentVersionLoaderTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IContentGrain>(id, null))
                .Returns(grain);

            sut = new ContentVersionLoader(grainFactory);
        }

        [Fact]
        public async Task Should_throw_exception_if_no_state_returned()
        {
            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(null));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.LoadAsync(id, 10));
        }

        [Fact]
        public async Task Should_throw_exception_if_state_has_other_version()
        {
            var content = new ContentEntity { Version = 5 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(content));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.LoadAsync(id, 10));
        }

        [Fact]
        public async Task Should_return_content_from_state()
        {
            var content = new ContentEntity { Version = 10 };

            A.CallTo(() => grain.GetStateAsync(10))
                .Returns(J.Of<IContentEntity>(content));

            var result = await sut.LoadAsync(id, 10);

            Assert.Same(content, result);
        }
    }
}
