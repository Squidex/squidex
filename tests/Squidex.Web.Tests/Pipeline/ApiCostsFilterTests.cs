// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.UsageTracking;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class ApiCostsFilterTests
    {
        private readonly IActionContextAccessor actionContextAccessor = A.Fake<IActionContextAccessor>();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly ActionExecutingContext actionContext;
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionExecutionDelegate next;
        private readonly ApiCostsFilter sut;
        private long apiCallsMax;
        private long apiCallsCurrent;
        private bool isNextCalled;

        public ApiCostsFilterTests()
        {
            actionContext =
                new ActionExecutingContext(
                    new ActionContext(httpContext, new RouteData(),
                        new ActionDescriptor()),
                    new List<IFilterMetadata>(), new Dictionary<string, object>(), null);

            A.CallTo(() => actionContextAccessor.ActionContext)
                .Returns(actionContext);

            A.CallTo(() => appPlansProvider.GetPlan(null))
                .Returns(appPlan);

            A.CallTo(() => appPlansProvider.GetPlanForApp(appEntity))
                .Returns(appPlan);

            A.CallTo(() => appPlan.MaxApiCalls)
                .ReturnsLazily(x => apiCallsMax);

            A.CallTo(() => usageTracker.GetMonthlyCallsAsync(A<string>.Ignored, DateTime.Today))
                .ReturnsLazily(x => Task.FromResult(apiCallsCurrent));

            next = () =>
            {
                isNextCalled = true;

                return Task.FromResult<ActionExecutedContext>(null);
            };

            sut = new ApiCostsFilter(appPlansProvider, usageTracker);
        }

        [Fact]
        public async Task Should_return_429_status_code_if_max_calls_over_limit()
        {
            sut.FilterDefinition = new ApiCostsAttribute(1);

            SetupApp();

            apiCallsCurrent = 1000;
            apiCallsMax = 600;

            await sut.OnActionExecutionAsync(actionContext, next);

            Assert.Equal(429, (actionContext.Result as StatusCodeResult).StatusCode);
            Assert.False(isNextCalled);

            A.CallTo(() => usageTracker.TrackAsync(A<string>.Ignored, A<string>.Ignored, A<double>.Ignored, A<double>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_track_if_calls_left()
        {
            sut.FilterDefinition = new ApiCostsAttribute(13);

            SetupApp();

            apiCallsCurrent = 1000;
            apiCallsMax = 1600;

            await sut.OnActionExecutionAsync(actionContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => usageTracker.TrackAsync(A<string>.Ignored, A<string>.Ignored, 13, A<double>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_allow_small_buffer()
        {
            sut.FilterDefinition = new ApiCostsAttribute(13);

            SetupApp();

            apiCallsCurrent = 1099;
            apiCallsMax = 1000;

            await sut.OnActionExecutionAsync(actionContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => usageTracker.TrackAsync(A<string>.Ignored, A<string>.Ignored, 13, A<double>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_track_if_weight_is_zero()
        {
            sut.FilterDefinition = new ApiCostsAttribute(0);

            SetupApp();

            apiCallsCurrent = 1000;
            apiCallsMax = 600;

            await sut.OnActionExecutionAsync(actionContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => usageTracker.TrackAsync(A<string>.Ignored, A<string>.Ignored, A<double>.Ignored, A<double>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_track_if_app_not_defined()
        {
            sut.FilterDefinition = new ApiCostsAttribute(1);

            apiCallsCurrent = 1000;
            apiCallsMax = 600;

            await sut.OnActionExecutionAsync(actionContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => usageTracker.TrackAsync(A<string>.Ignored, A<string>.Ignored, A<double>.Ignored, A<double>.Ignored))
                .MustNotHaveHappened();
        }

        private void SetupApp()
        {
            httpContext.Context().App = appEntity;
        }
    }
}