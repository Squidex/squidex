// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline;

public class SchemaResolverTests : GivenContext
{
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionContext actionContext;
    private readonly ActionExecutingContext actionExecutingContext;
    private readonly ActionExecutionDelegate next;
    private readonly ClaimsIdentity user = new ClaimsIdentity();
    private readonly SchemaResolver sut;
    private bool isNextCalled;

    public SchemaResolverTests()
    {
        actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
        });

        actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), this);
        actionExecutingContext.HttpContext = httpContext;
        actionExecutingContext.HttpContext.User = new ClaimsPrincipal(user);
        actionExecutingContext.HttpContext.Features.Set(App);

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };

        sut = new SchemaResolver(AppProvider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_return_404_if_schema_name_is_null(string? schema)
    {
        actionContext.RouteData.Values["schema"] = schema;

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertNotFound();

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, A<DomainId>._, true, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_404_if_schema_not_published_when_attribute_applied()
    {
        actionContext.ActionDescriptor.EndpointMetadata.Add(new SchemaMustBePublishedAttribute());
        actionContext.RouteData.Values["schema"] = SchemaId.Id.ToString();

        Schema = Schema.Unpublish();

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, A<DomainId>._, true, httpContext.RequestAborted))
            .Returns(Schema);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertNotFound();
    }

    [Fact]
    public async Task Should_resolve_schema_if_schema_not_published()
    {
        actionContext.RouteData.Values["schema"] = SchemaId.Id.ToString();

        Schema = Schema.Unpublish();

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, A<DomainId>._, true, httpContext.RequestAborted))
            .Returns(Schema);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertSchema();
    }

    [Fact]
    public async Task Should_return_404_if_schema_not_found()
    {
        actionContext.RouteData.Values["schema"] = SchemaId.Id.ToString();

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, A<DomainId>._, true, httpContext.RequestAborted))
            .Returns(Task.FromResult<Schema?>(null));

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertNotFound();
    }

    [Fact]
    public async Task Should_resolve_schema_from_id()
    {
        actionContext.RouteData.Values["schema"] = SchemaId.Id.ToString();

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, SchemaId.Id, true, httpContext.RequestAborted))
            .Returns(Schema);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertSchema();
    }

    [Fact]
    public async Task Should_resolve_schema_from_id_without_caching_if_frontend()
    {
        user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

        actionContext.RouteData.Values["schema"] = SchemaId.Id.ToString();

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, SchemaId.Id, false, httpContext.RequestAborted))
            .Returns(Schema);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertSchema();
    }

    [Fact]
    public async Task Should_resolve_schema_from_name()
    {
        actionContext.RouteData.Values["schema"] = SchemaId.Name;

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, SchemaId.Name, true, httpContext.RequestAborted))
            .Returns(Schema);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertSchema();
    }

    [Fact]
    public async Task Should_resolve_schema_from_name_without_caching_if_frontend()
    {
        user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

        actionContext.RouteData.Values["schema"] = SchemaId.Name;

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, SchemaId.Name, false, httpContext.RequestAborted))
            .Returns(Schema);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        AssertSchema();
    }

    [Fact]
    public async Task Should_do_nothing_if_app_feature_not_set()
    {
        actionExecutingContext.RouteData.Values["schema"] = SchemaId.Name;

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.True(isNextCalled);

        A.CallTo(() => AppProvider.GetAppAsync(A<string>._, false, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_do_nothing_if_parameter_not_set()
    {
        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.True(isNextCalled);

        A.CallTo(() => AppProvider.GetAppAsync(A<string>._, false, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    private void AssertNotFound()
    {
        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    private void AssertSchema()
    {
        Assert.Equal(Schema, actionContext.HttpContext.Features.Get<Schema>());
        Assert.True(isNextCalled);
    }
}
