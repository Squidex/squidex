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
using Squidex.Domain.Apps.Entities.TestHelpers;
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
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(new AppFeature(Mocks.App(appId)));

            next = () =>
            {
                isNextCalled = true;

                return Task.FromResult<ActionExecutedContext>(null!);
            };

            sut = new SchemaResolver(appProvider);
        }

        public static IEnumerable<object[]> FieldNames()
        {
            yield return new object[] { "schema" };
            yield return new object[] { "publishedSchema" };
        }

        [Fact]
        public async Task Should_return_not_found_if_schema_not_published_for_publishedSchema_argument()
        {
            actionContext.RouteData.Values["publishedSchema"] = schemaId.Id.ToString();

            var schema = CreateSchema(false);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssetNotFound();
        }

        [Fact]
        public async Task Should_resolve_schema_if_schema_not_published_for_schema_argument()
        {
            actionContext.RouteData.Values["schema"] = schemaId.Id.ToString();

            var schema = CreateSchema(false);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssertSchema(schema);
        }

        [Theory]
        [MemberData(nameof(FieldNames))]
        public async Task Should_return_not_found_if_schema_not_found(string parameter)
        {
            actionContext.RouteData.Values[parameter] = schemaId.Id.ToString();

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, true))
                .Returns(Task.FromResult<ISchemaEntity?>(null));

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssetNotFound();
        }

        [Theory]
        [InlineData("schema", false)]
        [InlineData("publishedSchema", true)]
        public async Task Should_return_not_found_if_schema_not_published(string parameter, bool expect404)
        {
            actionContext.RouteData.Values[parameter] = schemaId.Id.ToString();

            var schema = CreateSchema(false);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, A<DomainId>._, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            if (expect404)
            {
                AssetNotFound();
            }
            else
            {
                AssertSchema(schema);
            }
        }

        [Theory]
        [MemberData(nameof(FieldNames))]
        public async Task Should_resolve_schema_from_id(string parameter)
        {
            actionContext.RouteData.Values[parameter] = schemaId.Id.ToString();

            var schema = CreateSchema(true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssertSchema(schema);
        }

        [Theory]
        [MemberData(nameof(FieldNames))]
        public async Task Should_resolve_schema_from_id_without_caching_if_frontend(string parameter)
        {
            user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

            actionContext.RouteData.Values[parameter] = schemaId.Id.ToString();

            var schema = CreateSchema(true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Id, false))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssertSchema(schema);
        }

        [Theory]
        [MemberData(nameof(FieldNames))]
        public async Task Should_resolve_schema_from_name(string parameter)
        {
            actionContext.RouteData.Values[parameter] = schemaId.Name;

            var schema = CreateSchema(true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, true))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssertSchema(schema);
        }

        [Theory]
        [MemberData(nameof(FieldNames))]
        public async Task Should_resolve_schema_from_name_without_caching_if_frontend(string parameter)
        {
            user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

            actionContext.RouteData.Values[parameter] = schemaId.Name;

            var schema = CreateSchema(true);

            A.CallTo(() => appProvider.GetSchemaAsync(appId.Id, schemaId.Name, false))
                .Returns(schema);

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            AssertSchema(schema);
        }

        [Theory]
        [MemberData(nameof(FieldNames))]
        public async Task Should_do_nothing_if_app_feature_not_set(string parameter)
        {
            actionExecutingContext.HttpContext.Features.Set<IAppFeature>(null!);
            actionExecutingContext.RouteData.Values[parameter] = schemaId.Name;

            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>._, false))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_do_nothing_if_parameter_not_set()
        {
            await sut.OnActionExecutionAsync(actionExecutingContext, next);

            Assert.True(isNextCalled);

            A.CallTo(() => appProvider.GetAppAsync(A<string>._, false))
                .MustNotHaveHappened();
        }

        private void AssetNotFound()
        {
            Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
            Assert.False(isNextCalled);
        }

        private void AssertSchema(ISchemaEntity schema)
        {
            Assert.Equal(schema, actionContext.HttpContext.Features.Get<ISchemaFeature>().Schema);
            Assert.True(isNextCalled);
        }

        private ISchemaEntity CreateSchema(bool published)
        {
            var schema = new Schema(schemaId.Name);

            if (published)
            {
                schema = schema.Publish();
            }

            var schemaEntity = A.Fake<ISchemaEntity>();

            A.CallTo(() => schemaEntity.SchemaDef)
                .Returns(schema);

            A.CallTo(() => schemaEntity.Id)
                .Returns(schemaId.Id);

            return schemaEntity;
        }
    }
}
