// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class AppPermissionAttributeTests
    {
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly Mock<ClaimsPrincipal> mockUser = new Mock<ClaimsPrincipal>();
        private readonly Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
        private readonly Mock<IActionContextAccessor> actionContextAccessor = new Mock<IActionContextAccessor>();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
        private readonly ClaimsIdentity identity = new ClaimsIdentity();
        private readonly AppClient client = new AppClient("clientId", "secret", AppClientPermission.Reader);
        private readonly IAppFeature appFeature = A.Fake<IAppFeature>();
        private readonly IFeatureCollection features = new FeatureCollection();
        private readonly RouteData routeData = new RouteData();
        private readonly ActionExecutingContext context;
        private ActionContext actionContext;
        private Claim clientClaim;
        private Claim subjectClaim;
        private AppPermissionAttribute sut = new MustBeAppReaderAttribute();

        public AppPermissionAttributeTests()
        {
            actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor.Object);
            actionContextAccessor.Setup(x => x.ActionContext).Returns(actionContext);
            clientClaim = new Claim("client_id", $"test:clientId");
            subjectClaim = new Claim("sub", "user");

            var clients = ImmutableDictionary.CreateBuilder<string, AppClient>();
            clients.Add("clientId", client);
            var contributors = ImmutableDictionary.CreateBuilder<string, AppContributorPermission>();
            contributors.Add("user", AppContributorPermission.Owner);

            A.CallTo(() => appFeature.App).Returns(appEntity);
            A.CallTo(() => appEntity.Clients).Returns(new AppClients(clients.ToImmutable()));
            A.CallTo(() => appEntity.Contributors).Returns(new AppContributors(contributors.ToImmutable()));
            features.Set<IAppFeature>(appFeature);
            mockUser.Setup(x => x.Identities).Returns(new List<ClaimsIdentity> { identity });

            httpContextMock.Setup(x => x.Features).Returns(features);
            httpContextMock.Setup(x => x.User).Returns(mockUser.Object);

            context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
            context.Filters.Clear();
            sut = new MustBeAppDeveloperAttribute();
        }

        [Fact]
        public void Null_Permission_Returns_Not_Found()
        {
            // Arrange
            sut = new MustBeAppReaderAttribute();
            context.Filters.Add(sut);
            mockUser.Setup(x => x.FindFirst(OpenIdClaims.Subject)).Returns((Claim)null);
            clientClaim = new Claim("client_id", "test");
            mockUser.Setup(x => x.FindFirst(OpenIdClaims.ClientId)).Returns(clientClaim);

            // Act
            sut.OnActionExecuting(context);

            // Assert
            var result = context.Result as NotFoundResult;
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public void Lower_Permission_Returns_Forbidden()
        {
            // Arrange
            sut = new MustBeAppEditorAttribute();
            context.Filters.Add(sut);
            mockUser.Setup(x => x.FindFirst(OpenIdClaims.Subject)).Returns((Claim)null);
            mockUser.Setup(x => x.FindFirst(OpenIdClaims.ClientId)).Returns(clientClaim);

            // Act
            sut.OnActionExecuting(context);

            // Assert
            var result = context.Result as StatusCodeResult;
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public void Higher_Permission_Should_Get_All_Lesser_Permissions()
        {
            // Arrange
            sut = new MustBeAppOwnerAttribute();
            context.Filters.Add(sut);
            mockUser.Setup(x => x.FindFirst(OpenIdClaims.Subject)).Returns(subjectClaim);

            // Act
            sut.OnActionExecuting(context);

            // Assert
            var result = context.HttpContext.User.Identities.First()?.Claims;
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(Enum.GetNames(typeof(AppPermission)).Length, result.Count());
        }
    }
}
