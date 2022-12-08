// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.AspNetCore.Http;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline;

public class UsageMiddlewareTests
{
    private readonly IAppLogStore usageLog = A.Fake<IAppLogStore>();
    private readonly IUsageGate usageGate = A.Fake<IUsageGate>();
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

        sut = new UsageMiddleware(usageLog, usageGate)
        {
            Clock = clock
        };
    }

    [Fact]
    public async Task Should_not_track_if_app_not_defined()
    {
        await sut.InvokeAsync(httpContext, next);

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(A<IAppEntity>._, A<string>._, A<DateTime>._, A<double>._, A<long>._, A<long>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_track_if_call_blocked()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await sut.InvokeAsync(httpContext, next);

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, A<double>._, A<long>._, A<long>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_track_if_calls_left()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

        await sut.InvokeAsync(httpContext, next);

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, 13, A<long>._, A<long>._, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_request_bytes()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));
        httpContext.Request.ContentLength = 1024;

        await sut.InvokeAsync(httpContext, next);

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, 13, A<long>._, 1024, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_response_bytes_with_writer()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

        await sut.InvokeAsync(httpContext, async x =>
        {
            await x.Response.BodyWriter.WriteAsync(Encoding.Default.GetBytes("Hello World"), httpContext.RequestAborted);

            await next(x);
        });

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, 13, A<long>._, 11, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_response_bytes_with_stream()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(13));

        await sut.InvokeAsync(httpContext, async x =>
        {
            await x.Response.Body.WriteAsync(Encoding.Default.GetBytes("Hello World"), httpContext.RequestAborted);

            await next(x);
        });

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, 13, A<long>._, 11, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_response_bytes_with_file()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
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

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, 13, A<long>._, 11, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_track_if_costs_are_zero()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(0));

        await sut.InvokeAsync(httpContext, next);

        Assert.True(isNextCalled);

        var date = instant.ToDateTimeUtc().Date;

        A.CallTo(() => usageGate.TrackRequestAsync(app, A<string>._, date, A<double>._, A<long>._, A<long>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_log_request_even_if_costs_are_zero()
    {
        var app = Mocks.App(appId);

        httpContext.Features.Set<IAppFeature>(new AppFeature(app));
        httpContext.Features.Set<IApiCostsFeature>(new ApiCostsAttribute(0));

        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/my-path";

        await sut.InvokeAsync(httpContext, next);

        A.CallTo(() => usageLog.LogAsync(appId.Id,
            A<RequestLog>.That.Matches(x =>
                x.Timestamp == instant &&
                x.RequestMethod == "GET" &&
                x.RequestPath == "/my-path" &&
                x.Costs == 0),
            default))
            .MustHaveHappened();
    }
}
