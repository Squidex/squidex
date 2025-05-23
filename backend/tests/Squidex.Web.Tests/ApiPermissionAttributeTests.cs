﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Shared;
using Squidex.Shared.Identity;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web;

public class ApiPermissionAttributeTests : GivenContext
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
            FilterDescriptors = [],
        });

        actionExecutingContext = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), this);
        actionExecutingContext.HttpContext = httpContext;
        actionExecutingContext.HttpContext.User = new ClaimsPrincipal(user);

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };
    }

    [Fact]
    public void Should_use_custom_authorization_scheme()
    {
        var sut = new ApiPermissionAttribute();

        Assert.Equal(Constants.ApiSecurityScheme, sut.AuthenticationSchemes);
    }

    [Fact]
    public async Task Should_make_permission_check_with_app_feature()
    {
        actionExecutingContext.HttpContext.Features.Set(App);

        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.my-app"));

        SetContext();

        var sut = new ApiPermissionAttribute(PermissionIds.AppSchemasCreate);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Null(actionExecutingContext.Result);
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_make_permission_check_with_schema_feature()
    {
        actionExecutingContext.HttpContext.Features.Set(App);
        actionExecutingContext.HttpContext.Features.Set(Schema);

        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.my-app.schemas.my-schema"));

        SetContext();

        var sut = new ApiPermissionAttribute(PermissionIds.AppSchemasUpdate);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Null(actionExecutingContext.Result);
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_return_forbidden_if_user_has_wrong_permission()
    {
        actionExecutingContext.HttpContext.Features.Set(App);

        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

        SetContext();

        var sut = new ApiPermissionAttribute(PermissionIds.AppSchemasCreate);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Equal(403, (actionExecutingContext.Result as StatusCodeResult)?.StatusCode);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_forbidden_if_route_data_has_no_value()
    {
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

        SetContext();

        var sut = new ApiPermissionAttribute(PermissionIds.AppSchemasCreate);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Equal(403, (actionExecutingContext.Result as StatusCodeResult)?.StatusCode);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_forbidden_if_user_has_no_permission()
    {
        SetContext();

        var sut = new ApiPermissionAttribute(PermissionIds.AppSchemasCreate);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Equal(403, (actionExecutingContext.Result as StatusCodeResult)?.StatusCode);
        Assert.False(isNextCalled);
    }

    private void SetContext()
    {
        var context = new Context(new ClaimsPrincipal(actionExecutingContext.HttpContext.User), null!);

        actionExecutingContext.HttpContext.Features.Set(context);
    }
}
