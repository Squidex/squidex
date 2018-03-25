// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net;
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
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class AppApiTests
    {
        private readonly Mock<IActionContextAccessor> actionContextAccessor = new Mock<IActionContextAccessor>();
        private readonly RouteData routeData = new RouteData();
        private readonly ActionDescriptor actionDescriptor = new ActionDescriptor();
        private readonly IAppPlansProvider appPlanProvider = A.Fake<IAppPlansProvider>();
        private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
        private readonly long usage = 1;
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly IFeatureCollection features = new FeatureCollection();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
        private readonly IAppFeature appFeature = A.Fake<IAppFeature>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly ActionExecutingContext context;
        private readonly AppApiFilter sut;
        private ActionExecutionDelegate next;

        public AppApiTests()
        {
            var actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor);
            actionContextAccessor.Setup(x => x.ActionContext).Returns(actionContext);
            context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
            context.Filters.Add(new AppApiAttribute());
            context.RouteData.Values.Add("app", "appName");

            httpContextMock.Setup(x => x.Features).Returns(features);
            A.CallTo(() => appProvider.GetAppAsync("appName")).Returns(appEntity);

            sut = new AppApiFilter(appProvider);
        }

        [Fact]
        public async Task Should_set_features_if_app_found()
        {
            next = new ActionExecutionDelegate(async () =>
            {
                return null;
            });
            await sut.OnActionExecutionAsync(context, next);

            Assert.NotEmpty(context.HttpContext.Features);
        }

        [Fact]
        public async Task Should_return_not_found_result_if_app_not_found()
        {
            next = new ActionExecutionDelegate(async () =>
            {
                return null;
            });

            A.CallTo(() => appProvider.GetAppAsync("appName")).Returns((IAppEntity)null);
            await sut.OnActionExecutionAsync(context, next);

            var result = context.Result as NotFoundResult;
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
