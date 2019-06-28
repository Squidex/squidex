// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;
using Xunit;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline
{
    public class AppResolverTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionContext actionContext;
        private readonly ActionExecutingContext actionExecutingContext;
        private readonly ActionExecutionDelegate next;
        private readonly ClaimsIdentity user = new ClaimsIdentity();
        private readonly string appName = "my-app";
        private readonly AppResolver sut;
        private bool isNextCalled;

        public AppResolverTests()
        {
            actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
            {
                FilterDescriptors = new List<FilterDescriptor>()
            });

            actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), this);
            actionExecutingContext.HttpContext = httpContext;
            actionExecutingContext.HttpContext.User = new ClaimsPrincipal(user);
            actionExecutingContext.RouteData.Values["app"] = appName;

            next = () =>
            {
                isNextCalled = true;

                return Task.FromResult<ActionExecutedContext>(null);
            };

            sut = new AppResolver(appProvider);
        }

        [Fact]
        public async Task Should_return_not_found_if_app_not_found()
        {
            A.CallTo(() => appProvider.GetAppAsync(appName))
                .Returns(Task.FromResult<IAppEntity>(null));

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_app_from_user()
        {
            var app = CreateApp(appName, appUser: "user1");

            user.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));
            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.my-app"));

            A.CallTo(() => appProvider.GetAppAsync(appName))
                .Returns(app);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Same(app, httpContext.Context().App);
            Assert.True(user.Claims.Count() > 2);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_app_from_client()
        {
            var app = CreateApp(appName, appClient: "client1");

            user.AddClaim(new Claim(OpenIdClaims.ClientId, "client1"));
            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.my-app"));

            A.CallTo(() => appProvider.GetAppAsync(appName))
                .Returns(app);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Same(app, httpContext.Context().App);
            Assert.True(user.Claims.Count() > 2);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_app_if_anonymouse_but_not_permissions()
        {
            var app = CreateApp(appName);

            user.AddClaim(new Claim(OpenIdClaims.ClientId, "client1"));
            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

            actionContext.ActionDescriptor.FilterDescriptors.Add(new FilterDescriptor(new AllowAnonymousFilter(), 1));

            A.CallTo(() => appProvider.GetAppAsync(appName))
                .Returns(app);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Same(app, httpContext.Context().App);
            Assert.Equal(2, user.Claims.Count());
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_return_not_found_if_user_has_no_permissions()
        {
            var app = CreateApp(appName);

            user.AddClaim(new Claim(OpenIdClaims.ClientId, "client1"));
            user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

            A.CallTo(() => appProvider.GetAppAsync(appName))
                .Returns(app);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_do_nothing_if_parameter_not_set()
        {
            actionExecutingContext.RouteData.Values.Remove("app");

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>.Ignored))
                .MustNotHaveHappened();
        }

        private static IAppEntity CreateApp(string name, string appUser = null, string appClient = null)
        {
            var appEntity = A.Fake<IAppEntity>();

            if (appUser != null)
            {
                A.CallTo(() => appEntity.Contributors)
                    .Returns(AppContributors.Empty.Assign(appUser, Role.Owner));
            }
            else
            {
                A.CallTo(() => appEntity.Contributors)
                    .Returns(AppContributors.Empty);
            }

            if (appClient != null)
            {
                A.CallTo(() => appEntity.Clients)
                    .Returns(AppClients.Empty.Add(appClient, "secret"));
            }
            else
            {
                A.CallTo(() => appEntity.Clients)
                    .Returns(AppClients.Empty);
            }

            A.CallTo(() => appEntity.Roles)
                .Returns(Roles.CreateDefaults(name));

            return appEntity;
        }
    }
}
