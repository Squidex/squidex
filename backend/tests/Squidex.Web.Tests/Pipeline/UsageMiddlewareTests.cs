// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class UsageMiddlewareTests
    {
        private readonly IAppLogStore appLogStore = A.Fake<IAppLogStore>();
        private readonly IApiUsageTracker usageTracker = A.Fake<IApiUsageTracker>();
        private readonly IClock clock = A.Fake<IClock>();
        private readonly Instant instant = SystemClock.Instance.GetCurrentInstant();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly RequestDelegate next;
        private readonly UsageMiddleware sut;
        private bool isNextCalled;

        public UsageMiddlewareTests()
        {
            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(instant);

            next = x =>
            {
                isNextCalled = true;

                return Task.CompletedTask;
            };

            sut = new UsageMiddleware(appLogStore, usageTracker, clock);
        }

        [Fact]
        public async Task Should_not_track_if_app_not_defined()
        {
            await sut.InvokeAsync(httpContext, next);

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, A<double>._, A<long>._, A<long>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_track_if_call_blocked()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            await sut.InvokeAsync(httpContext, next);

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, A<double>._, A<long>._, A<long>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_track_if_calls_left()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

            await sut.InvokeAsync(httpContext, next);

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, 13, A<long>._, A<long>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_track_request_bytes()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));
            httpContext.Request.ContentLength = 1024;

            await sut.InvokeAsync(httpContext, next);

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, 13, A<long>._, 1024, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_track_response_bytes_with_writer()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

            await sut.InvokeAsync(httpContext, async x =>
            {
                await x.Response.BodyWriter.WriteAsync(Encoding.Default.GetBytes("Hello World"), httpContext.RequestAborted);

                await next(x);
            });

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, 13, A<long>._, 11, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_track_response_bytes_with_stream()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

            await sut.InvokeAsync(httpContext, async x =>
            {
                await x.Response.Body.WriteAsync(Encoding.Default.GetBytes("Hello World"), httpContext.RequestAborted);

                await next(x);
            });

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, 13, A<long>._, 11, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_track_response_bytes_with_file()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

            var tempFileName = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempFileName, "Hello World", httpContext.RequestAborted);

                await sut.InvokeAsync(httpContext, async x =>
                {
                    await x.Response.SendFileAsync(tempFileName, 0, new FileInfo(tempFileName).Length, httpContext.RequestAborted);

                    await next(x);
                });
            }
            finally
            {
                File.Delete(tempFileName);
            }

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, 13, A<long>._, 11, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_track_if_costs_are_zero()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(0));

            await sut.InvokeAsync(httpContext, next);

            Assert.True(isNextCalled);

            var date = instant.ToDateTimeUtc().Date;

            A.CallTo(() => usageTracker.TrackAsync(date, A<string>._, A<string>._, A<double>._, A<long>._, A<long>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_log_request_even_if_costs_are_zero()
        {
            httpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));
            httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(0));

            httpContext.Request.Method = "GET";
            httpContext.Request.Path = "/my-path";

            await sut.InvokeAsync(httpContext, next);

            A.CallTo(() => appLogStore.LogAsync(appId.Id,
                A<RequestLog>.That.Matches(x =>
                    x.Timestamp == instant &&
                    x.RequestMethod == "GET" &&
                    x.RequestPath == "/my-path" &&
                    x.Costs == 0),
                default))
                .MustHaveHappened();
        }
    }
}
