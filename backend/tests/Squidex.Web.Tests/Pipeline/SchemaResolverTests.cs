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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Xunit;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline
{
    public class SchemaResolverTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionContext actionContext;
        private readonly ActionExecutingContext actionExecutingContext;
        private readonly ActionExecutionDelegate next;
        private readonly ClaimsIdentity user = new ClaimsIdentity();
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly SchemaResolver sut;
        private bool isNextCalled;

        public SchemaResolverTests()
        {
            actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
            {
                EndpointMetadata = new List<object>()
            });

            actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), this);
            actionExecutingContext.HttpContext = httpContext;
            actionExecutingContext.HttpContext.User = new ClaimsPrincipal(user);
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(new AppFeature(appId));

            next = () =>
            {
                isNextCalled = true;

                return Task.FromResult<ActionExecutedContext>(null!);
            };

            sut = new SchemaResolver(appProvider);
        }

        [Fact]
        public async Task Should_return_not_found_if_schema_not_found()
        {
            actionContext.RouteData.Values["name"] = schemaId.Id.ToString();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, false, true))
                .Returns(Task.FromResult<ISchemaEntity?>(null));

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_schema_from_id()
        {
            actionContext.RouteData.Values["name"] = schemaId.Id.ToString();

            var schema = CreateSchema();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(schemaId, actionContext.HttpContext.Features.Get<ISchemaFeature>().SchemaId);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_schema_from_id_without_caching_if_frontend()
        {
            user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

            actionContext.RouteData.Values["name"] = schemaId.Id.ToString();

            var schema = CreateSchema();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false, false))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(schemaId, actionContext.HttpContext.Features.Get<ISchemaFeature>().SchemaId);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_schema_from_name()
        {
            actionContext.RouteData.Values["name"] = schemaId.Name;

            var schema = CreateSchema();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(schemaId, actionContext.HttpContext.Features.Get<ISchemaFeature>().SchemaId);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_schema_from_name_without_caching_if_frontend()
        {
            user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

            actionContext.RouteData.Values["name"] = schemaId.Name;

            var schema = CreateSchema();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, false))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(schemaId, actionContext.HttpContext.Features.Get<ISchemaFeature>().SchemaId);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_do_nothing_if_parameter_not_set()
        {
            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>._, false))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_do_nothing_if_app_feature_not_set()
        {
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(null!);
            actionExecutingContext.RouteData.Values["name"] = schemaId.Name;

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>._, false))
                .MustNotHaveHappened();
        }

        private ISchemaEntity CreateSchema()
        {
            var schemaEntity = A.Fake<ISchemaEntity>();

            A.CallTo(() => schemaEntity.SchemaDef)
                .Returns(new Schema(schemaId.Name));

            A.CallTo(() => schemaEntity.Id)
                .Returns(schemaId.Id);

            return schemaEntity;
        }
    }
}
