// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class CachingFilterTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionExecutingContext executingContext;
        private readonly ActionExecutedContext executedContext;
        private readonly CachingOptions cachingOptions = new CachingOptions();
        private readonly CachingManager cachingManager;
        private readonly CachingFilter sut;

        public CachingFilterTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            cachingManager = new CachingManager(httpContextAccessor, Options.Create(cachingOptions));

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var actionFilters = new List<IFilterMetadata>();

            executingContext = new ActionExecutingContext(actionContext, actionFilters, new Dictionary<string, object>(), this);
            executedContext = new ActionExecutedContext(actionContext, actionFilters, this)
            {
                Result = new OkResult()
            };

            sut = new CachingFilter(cachingManager, Options.Create(cachingOptions));
        }

        [Fact]
        public async Task Should_not_append_etag_if_not_found()
        {
            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(StringValues.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_append_authorization_header_as_vary()
        {
            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("Auth-State", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_append_authorization_as_header_when_user_has_subject()
        {
            var identity = (ClaimsIdentity)httpContext.User.Identity!;

            identity.AddClaim(new Claim(OpenIdClaims.Subject, "my-id"));

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("Auth-State,Authorization", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_append_client_id_as_header_when_user_has_client_but_no_subject()
        {
            var identity = (ClaimsIdentity)httpContext.User.Identity!;

            identity.AddClaim(new Claim(OpenIdClaims.ClientId, "my-client"));

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("Auth-State,Auth-ClientId", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_not_append_null_header_as_vary()
        {
            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddHeader(null!);

                return Task.FromResult(executedContext);
            });

            Assert.Equal("Auth-State", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_not_append_empty_header_as_vary()
        {
            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddHeader(string.Empty);

                return Task.FromResult(executedContext);
            });

            Assert.Equal("Auth-State", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_append_custom_header_as_vary()
        {
            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddHeader("X-Header");

                return Task.FromResult(executedContext);
            });

            Assert.Equal("Auth-State,X-Header", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_not_append_etag_if_empty()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = string.Empty;

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(string.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_strong_etag_if_disabled()
        {
            cachingOptions.StrongETag = true;

            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_already_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = "W/13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("W/13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_convert_strong_to_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("W/13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_empty_string_to_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = string.Empty;

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(string.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_return_304_for_same_etags()
        {
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "W/13";

            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(304, ((StatusCodeResult)executedContext.Result).StatusCode);
        }

        [Fact]
        public async Task Should_not_return_304_for_different_etags()
        {
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "W/11";

            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(200, ((StatusCodeResult)executedContext.Result).StatusCode);
        }

        [Fact]
        public async Task Should_append_surrogate_keys()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            cachingOptions.MaxSurrogateKeysSize = 100;

            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);

                return Task.FromResult(executedContext);
            });

            Assert.Equal($"{id1} {id2}", httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_append_surrogate_keys_if_just_enough_space_for_one()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            cachingOptions.MaxSurrogateKeysSize = 36;

            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);

                return Task.FromResult(executedContext);
            });

            Assert.Equal($"{id1}", httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_not_append_surrogate_keys_if_maximum_is_exceeded()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            cachingOptions.MaxSurrogateKeysSize = 20;

            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);

                return Task.FromResult(executedContext);
            });

            Assert.Equal(StringValues.Empty, httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_not_append_surrogate_keys_if_maximum_is_overriden()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            httpContext.Request.Headers[CachingManager.SurrogateKeySizeHeader] = "20";

            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);

                return Task.FromResult(executedContext);
            });

            Assert.Equal(StringValues.Empty, httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_generate_etag_from_ids_and_versions()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
                cachingManager.AddDependency(12);

                return Task.FromResult(executedContext);
            });

            Assert.True(httpContext.Response.Headers[HeaderNames.ETag].ToString().Length > 20);
        }

        [Fact]
        public async Task Should_not_generate_etag_when_already_added()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            await sut.OnActionExecutionAsync(executingContext, () =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
                cachingManager.AddDependency(12);

                executedContext.HttpContext.Response.Headers[HeaderNames.ETag] = "W/20";

                return Task.FromResult(executedContext);
            });

            Assert.Equal("W/20", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        private ActionExecutionDelegate Next()
        {
            return () => Task.FromResult(executedContext);
        }
    }
}
