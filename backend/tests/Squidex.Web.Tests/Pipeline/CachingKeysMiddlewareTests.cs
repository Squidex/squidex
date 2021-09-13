// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class CachingKeysMiddlewareTests
    {
        private readonly List<(object, Func<object, Task>)> callbacks = new List<(object, Func<object, Task>)>();
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly IHttpResponseBodyFeature httpResponseBodyFeature = A.Fake<IHttpResponseBodyFeature>();
        private readonly IHttpResponseFeature httpResponseFeature = A.Fake<IHttpResponseFeature>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly CachingOptions cachingOptions = new CachingOptions();
        private readonly CachingManager cachingManager;
        private readonly RequestDelegate next;
        private readonly CachingKeysMiddleware sut;
        private bool isNextCalled;

        public CachingKeysMiddlewareTests()
        {
            var headers = new HeaderDictionary();

            A.CallTo(() => httpResponseFeature.Headers)
                .Returns(headers);

            A.CallTo(() => httpResponseFeature.OnStarting(A<Func<object, Task>>._, A<object>._))
                .Invokes(c =>
                {
                    callbacks.Add((
                        c.GetArgument<object>(1)!,
                        c.GetArgument<Func<object, Task>>(0)!));
                });

            A.CallTo(() => httpResponseBodyFeature.StartAsync(A<CancellationToken>._))
                .Invokes(c =>
                {
                    foreach (var (state, callback) in callbacks)
                    {
                        callback(state).Wait(httpContext.RequestAborted);
                    }
                });

            httpContext.Features.Set(httpResponseBodyFeature);
            httpContext.Features.Set(httpResponseFeature);

            next = context =>
            {
                isNextCalled = true;

                return Task.CompletedTask;
            };

            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            cachingManager = new CachingManager(httpContextAccessor, Options.Create(cachingOptions));

            sut = new CachingKeysMiddleware(cachingManager, Options.Create(cachingOptions), next);
        }

        [Fact]
        public async Task Should_invoke_next()
        {
            await MakeRequestAsync();

            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_not_append_etag_if_not_found()
        {
            await MakeRequestAsync();

            Assert.Equal(StringValues.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_append_authorization_header_as_vary()
        {
            await MakeRequestAsync();

            Assert.Equal("Auth-State", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_append_authorization_as_header_if_user_has_subject()
        {
            var identity = (ClaimsIdentity)httpContext.User.Identity!;

            identity.AddClaim(new Claim(OpenIdClaims.Subject, "my-id"));

            await MakeRequestAsync();

            Assert.Equal("Auth-State,Authorization", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_append_client_id_as_header_if_user_has_client_but_no_subject()
        {
            var identity = (ClaimsIdentity)httpContext.User.Identity!;

            identity.AddClaim(new Claim(OpenIdClaims.ClientId, "my-client"));

            await MakeRequestAsync();

            Assert.Equal("Auth-State,Auth-ClientId", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_not_append_null_header_as_vary()
        {
            await MakeRequestAsync(() =>
            {
                cachingManager.AddHeader(null!);
            });

            Assert.Equal("Auth-State", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_not_append_empty_header_as_vary()
        {
            await MakeRequestAsync(() =>
            {
                cachingManager.AddHeader(string.Empty);
            });

            Assert.Equal("Auth-State", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_append_custom_header_as_vary()
        {
            await MakeRequestAsync(() =>
            {
                cachingManager.AddHeader("X-Header");
            });

            Assert.Equal("Auth-State,X-Header", httpContext.Response.Headers[HeaderNames.Vary]);
        }

        [Fact]
        public async Task Should_not_append_etag_if_empty()
        {
            await MakeRequestAsync(() =>
            {
                httpContext.Response.Headers[HeaderNames.ETag] = string.Empty;
            });

            Assert.Equal(string.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_strong_etag_if_disabled()
        {
            cachingOptions.StrongETag = true;

            await MakeRequestAsync(() =>
            {
                httpContext.Response.Headers[HeaderNames.ETag] = "13";
            });

            Assert.Equal("13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_already_weak_tag()
        {
            await MakeRequestAsync(() =>
            {
                httpContext.Response.Headers[HeaderNames.ETag] = "W/13";
            });

            Assert.Equal("W/13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_convert_strong_to_weak_tag()
        {
            await MakeRequestAsync(() =>
            {
                httpContext.Response.Headers[HeaderNames.ETag] = "13";
            });

            Assert.Equal("W/13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_empty_string_to_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = string.Empty;

            await MakeRequestAsync();

            Assert.Equal(string.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_append_surrogate_keys()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            cachingOptions.MaxSurrogateKeysSize = 100;

            await MakeRequestAsync(() =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
            });

            Assert.Equal($"{id1} {id2}", httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_append_surrogate_keys_if_just_enough_space_for_one()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            cachingOptions.MaxSurrogateKeysSize = 36;

            await MakeRequestAsync(() =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
            });

            Assert.Equal($"{id1}", httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_not_append_surrogate_keys_if_maximum_is_exceeded()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            cachingOptions.MaxSurrogateKeysSize = 20;

            await MakeRequestAsync(() =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
            });

            Assert.Equal(StringValues.Empty, httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_not_append_surrogate_keys_if_maximum_is_overriden()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            httpContext.Request.Headers[CachingManager.SurrogateKeySizeHeader] = "20";

            await MakeRequestAsync(() =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
            });

            Assert.Equal(StringValues.Empty, httpContext.Response.Headers["Surrogate-Key"]);
        }

        [Fact]
        public async Task Should_generate_etag_from_ids_and_versions()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            await MakeRequestAsync(() =>
            {
                cachingManager.AddDependency(id1, 12);
                cachingManager.AddDependency(id2, 12);
                cachingManager.AddDependency(12);
            });

            Assert.True(httpContext.Response.Headers[HeaderNames.ETag].ToString().Length > 20);
        }

        [Fact]
        public async Task Should_not_generate_etag_if_already_added()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            await MakeRequestAsync(() =>
            {
                cachingManager.AddDependency(DomainId.NewGuid(), 12);
                cachingManager.AddDependency(DomainId.NewGuid(), 12);
                cachingManager.AddDependency(12);

                httpContext.Response.Headers[HeaderNames.ETag] = "W/20";
            });

            Assert.Equal("W/20", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        private async Task MakeRequestAsync(Action? action = null)
        {
            await sut.InvokeAsync(httpContext);

            action?.Invoke();

            await httpContext.Response.StartAsync(httpContext.RequestAborted);
        }
    }
}
