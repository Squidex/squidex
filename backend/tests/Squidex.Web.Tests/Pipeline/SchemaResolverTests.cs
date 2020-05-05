// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
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

            next = () =>
            {
                isNextCalled = true;

                return Task.FromResult<ActionExecutedContext?>(null);
            };

            sut = new SchemaResolver(appProvider);
        }

        [Fact]
        public async Task Should_return_not_found_if_schema_not_found()
        {
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(new AppFeature(appId));
            actionContext.RouteData.Values["name"] = schemaId.Id.ToString();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(Task.FromResult<ISchemaEntity?>(null));

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_schema_from_id()
        {
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(new AppFeature(appId));
            actionContext.RouteData.Values["name"] = schemaId.Id.ToString();

            var schema = CreateSchema();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(schemaId, actionContext.HttpContext.Features.Get<ISchemaFeature>().SchemaId);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_resolve_schema_from_name()
        {
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(new AppFeature(appId));
            actionContext.RouteData.Values["name"] = schemaId.Name;

            var schema = CreateSchema();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.Equal(schemaId, actionContext.HttpContext.Features.Get<ISchemaFeature>().SchemaId);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_do_nothing_if_app_feature_not_set()
        {
            actionExecutingContext.RouteData.Values["name"] = schemaId.Name;

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_do_nothing_if_parameter_not_set()
        {
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(new AppFeature(appId));
            actionExecutingContext.RouteData.Values.Remove("name");

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>._))
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
