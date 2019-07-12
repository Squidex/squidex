// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web
{
    public class ApiPermissionAttributeTests
    {
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionExecutingContext actionExecutingContext;
        private readonly ActionExecutionDelegate next;
        private readonly ClaimsIdentity user = new ClaimsIdentity();
        private bool isNextCalled;

        public ApiPermissionAttributeTests()
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
            {
                FilterDescriptors = new List<FilterDescriptor>()
            });

            actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), this);
            actionExecutingContext.HttpContext = httpContext;
            actionExecutingContext.HttpContext.Context().User = new ClaimsPrincipal(user);

            next = () =>
            {
                isNextCalled = true;

                return Task.FromResult<ActionExecutedContext>(null);
            };
        }

        [Fact]
        public void Should_use_bearer_schemes()
        {
            var sut = new ApiPermissionAttribute();

            Assert.Equal("Bearer", sut.AuthenticationSchemes);
        }

        [Fact]
        public async Task Should_call_next_when_user_has_correct_permission()
        {
            actionExecutingContext.RouteData.Values["app"] = "my-app";

            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.my-app"));

            var sut = new ApiPermissionAttribute(Permissions.AppSchemasCreate);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Null(actionExecutingContext.Result);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_return_forbidden_when_user_has_wrong_permission()
        {
            actionExecutingContext.RouteData.Values["app"] = "my-app";

            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

            var sut = new ApiPermissionAttribute(Permissions.AppSchemasCreate);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(403, (actionExecutingContext.Result as StatusCodeResult)?.StatusCode);
            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_return_forbidden_when_route_data_has_no_value()
        {
            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

            var sut = new ApiPermissionAttribute(Permissions.AppSchemasCreate);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(403, (actionExecutingContext.Result as StatusCodeResult)?.StatusCode);
            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_return_forbidden_when_user_has_no_permission()
        {
            var sut = new ApiPermissionAttribute(Permissions.AppSchemasCreate);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(403, (actionExecutingContext.Result as StatusCodeResult)?.StatusCode);
            Assert.False(isNextCalled);
        }
    }
}
