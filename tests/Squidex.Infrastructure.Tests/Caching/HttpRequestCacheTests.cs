// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Squidex.Infrastructure.Caching
{
    public class HttpRequestCacheTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly IRequestCache sut;
        private int called;

        public HttpRequestCacheTests()
        {
            sut = new HttpRequestCache(httpContextAccessor);
        }

        [Fact]
        public void Should_add_item_to_cache_when_context_exists()
        {
            SetupContext();

            sut.Add("Key", 1);

            var found = sut.TryGetValue("Key", out var value);

            Assert.True(found);
            Assert.Equal(1, value);

            sut.Remove("Key");

            var foundAfterRemove = sut.TryGetValue("Key", out value);

            Assert.False(foundAfterRemove);
            Assert.Null(value);
        }

        [Fact]
        public void Should_not_add_item_to_cache_when_context_not_exists()
        {
            SetupNoContext();

            sut.Add("Key", 1);

            var found = sut.TryGetValue("Key", out var value);

            Assert.False(found);
            Assert.Null(value);

            sut.Remove("Key");

            var foundAfterRemove = sut.TryGetValue("Key", out value);

            Assert.False(foundAfterRemove);
            Assert.Null(value);
        }

        [Fact]
        public void Should_call_creator_once_when_context_exists()
        {
            SetupContext();

            var value1 = sut.GetOrCreate("Key", () => ++called);
            var value2 = sut.GetOrCreate("Key", () => ++called);

            Assert.Equal(1, called);
            Assert.Equal(1, value1);
            Assert.Equal(1, value2);
        }

        [Fact]
        public void Should_call_creator_twice_when_context_not_exists()
        {
            SetupNoContext();

            var value1 = sut.GetOrCreate("Key", () => ++called);
            var value2 = sut.GetOrCreate("Key", () => ++called);

            Assert.Equal(2, called);
            Assert.Equal(1, value1);
            Assert.Equal(2, value2);
        }

        [Fact]
        public async Task Should_call_async_creator_once_when_context_exists()
        {
            SetupContext();

            var value1 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));
            var value2 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

            Assert.Equal(1, called);
            Assert.Equal(1, value1);
            Assert.Equal(1, value2);
        }

        [Fact]
        public async Task Should_call_async_creator_twice_when_context_not_exists()
        {
            SetupNoContext();

            var value1 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));
            var value2 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

            Assert.Equal(2, called);
            Assert.Equal(1, value1);
            Assert.Equal(2, value2);
        }

        private void SetupNoContext()
        {
            A.CallTo(() => httpContextAccessor.HttpContext).Returns(null);
        }

        private void SetupContext()
        {
            var httpItems = new Dictionary<object, object>();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Items).Returns(httpItems);
            A.CallTo(() => httpContextAccessor.HttpContext).Returns(httpContext);
        }
    }
}
