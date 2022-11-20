// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web.Pipeline;

public class ApiPermissionUnifierTests
{
    private readonly ApiPermissionUnifier sut = new ApiPermissionUnifier();

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    public async Task Should_add_admin_permission_if_user_is_in_role(string role)
    {
        var userIdentity = new ClaimsIdentity();
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        userIdentity.AddClaim(new Claim(userIdentity.RoleClaimType, role));

        var actual = await sut.TransformAsync(userPrincipal);

        Assert.Equal(PermissionIds.Admin, actual.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.Permissions)?.Value);
        Assert.Equal(role, actual.Claims.FirstOrDefault(x => x.Type == userIdentity.RoleClaimType)?.Value);
    }

    [Fact]
    public async Task Should_not_add_admin_persmission_if_user_has_other_role()
    {
        var userIdentity = new ClaimsIdentity();
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        userIdentity.AddClaim(new Claim(userIdentity.RoleClaimType, "Developer"));

        var actual = await sut.TransformAsync(userPrincipal);

        Assert.Single(actual.Claims);
    }

    [Fact]
    public async Task Should_not_add_admin_persmission_if_user_has_no_role()
    {
        var userIdentity = new ClaimsIdentity();
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        var actual = await sut.TransformAsync(userPrincipal);

        Assert.Empty(actual.Claims);
    }
}
