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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline;
using Xunit;
using static Squidex.Pipeline.AppApiFilter;

namespace Squidex.Tests.Pipeline
{
    public class ApiCostTests
    {
        private readonly Mock<IActionContextAccessor> actionContextAccessor = new Mock<IActionContextAccessor>();
        private readonly RouteData routeData = new RouteData();
        private readonly Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
        private readonly IAppPlansProvider appPlanProvider = A.Fake<IAppPlansProvider>();
        private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
        private readonly long usage = 1;
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly IFeatureCollection features = new FeatureCollection();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
        private readonly IAppFeature appFeature = A.Fake<IAppFeature>();
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly Guid appId = Guid.NewGuid();
        private ActionExecutingContext context;
        private ActionExecutionDelegate next;
        private ApiCostsFilter sut;

        public ApiCostTests()
        {
            var actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor.Object);
            actionContextAccessor.Setup(x => x.ActionContext).Returns(actionContext);
            context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
            context.Filters.Add(new ServiceFilterAttribute(typeof(ApiCostsFilter)));

            A.CallTo(() => appEntity.Id).Returns(appId);
            A.CallTo(() => appFeature.App).Returns(appEntity);

            features.Set<IAppFeature>(new AppFeature(appEntity));
            httpContextMock.Setup(x => x.Features).Returns(features);
            A.CallTo(() => usageTracker.GetMonthlyCalls(appId.ToString(), DateTime.Today))
                .Returns(usage);
        }

        [Fact]
        public async Task Should_return_429_status_code_if_max_calls_over_limit()
        {
            SetupSystem(2, 1);

            next = new ActionExecutionDelegate(async () =>
            {
                return null;
            });
            await sut.OnActionExecutionAsync(context, next);

            Assert.Equal(new StatusCodeResult(429).StatusCode, (context.Result as StatusCodeResult).StatusCode);
        }

        [Fact]
        public async Task Should_call_next_if_weight_is_0()
        {
            SetupSystem(0, 1);

            var result = 0;
            next = new ActionExecutionDelegate(async () =>
            {
                result = 1;
                return null;
            });
            await sut.OnActionExecutionAsync(context, next);

            Assert.Equal(1, result);
        }

        private ApiCostsFilter SetupSystem(double weight, long maxCalls)
        {
            A.CallTo(() => appPlan.MaxApiCalls).Returns(maxCalls);
            A.CallTo(() => appPlanProvider.GetPlanForApp(appFeature.App)).Returns(appPlan);

            sut = new ApiCostsFilter(appPlanProvider, usageTracker);
            sut.FilterDefinition = new ApiCostsAttribute(weight);

            return sut;
        }
    }
}
