// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

public class JintUserTests
{
    [Fact]
    public void Should_set_user_id_from_client_id()
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim(OpenIdClaims.ClientId, "1"));

        AssertUser(identity, "1", true, false);
    }

    [Fact]
    public void Should_set_user_id_from_subject_id()
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim(OpenIdClaims.Subject, "2"));
        identity.AddClaim(new Claim(OpenIdClaims.Name, "user"));

        AssertUser(identity, "2", false, true);
    }

    [Fact]
    public void Should_set_email_from_claim()
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim(OpenIdClaims.Email, "hello@squidex.io"));

        const string script = @"
                return user.email;
            ";

        Assert.Equal("hello@squidex.io", GetValue(identity, script));
    }

    [Fact]
    public void Should_simplify_squidex_claims()
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim(SquidexClaimTypes.PictureUrl, "my-picture"));

        const string script = @"
                return user.claims.picture;
            ";

        Assert.Equal(new[] { "my-picture" }, GetValue(identity, script));
    }

    [Fact]
    public void Should_simplify_default_claims()
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim(ClaimTypes.Role, "my-role"));

        const string script = @"
                return user.claims.role;
            ";

        Assert.Equal(new[] { "my-role" }, GetValue(identity, script));
    }

    [Fact]
    public void Should_set_claims()
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim("prefix1.claim1", "1a"));
        identity.AddClaim(new Claim("prefix2.claim1", "1b"));
        identity.AddClaim(new Claim("claim2", "2a"));
        identity.AddClaim(new Claim("claim2", "2b"));

        const string script1 = @"
                return user.claims.claim1;
            ";

        const string script2 = @"
                return user.claims.claim2;
            ";

        Assert.Equal(new[] { "1a", "1b" }, GetValue(identity, script1));
        Assert.Equal(new[] { "2a", "2b" }, GetValue(identity, script2));
    }

    private static void AssertUser(ClaimsIdentity identity, string id, bool isClient, bool isUser)
    {
        Assert.Equal(id, GetValue(identity, "user.id"));
        Assert.Equal(isUser, GetValue(identity, "user.isUser"));
        Assert.Equal(isClient, GetValue(identity, "user.isClient"));
    }

    private static object GetValue(ClaimsIdentity identity, string script)
    {
        var engine = new Engine();

        engine.SetValue("user", JintUser.Create(engine, new ClaimsPrincipal(new[] { identity })));

        return engine.Evaluate(script).ToObject();
    }
}
