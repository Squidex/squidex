// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Users;

public class SquidexClaimExtensionsTests
{
    [Fact]
    public void Should_extract_custom_claims_v1()
    {
        var source = new[]
        {
            new Claim($"{SquidexClaimTypes.Custom}:key1", "value1"),
            new Claim($"{SquidexClaimTypes.Custom}:key2", "value2 "),
        };

        var result = source.GetCustomProperties();

        Assert.Equal(new[]
        {
            ("key1", "value1"),
            ("key2", "value2")
        }, result.ToArray());
    }

    [Fact]
    public void Should_extract_custom_claims_v2()
    {
        var source = new[]
        {
            new Claim(SquidexClaimTypes.Custom, "key1=value1"),
            new Claim(SquidexClaimTypes.Custom, "key2=value2 "),
            new Claim(SquidexClaimTypes.Custom, "value "),
        };

        var result = source.GetCustomProperties();

        Assert.Equal(new[]
        {
            ("key1", "value1"),
            ("key2", "value2")
        }, result.ToArray());
    }

    [Fact]
    public void Should_extract_ui_claims_v1()
    {
        var source = new[]
        {
            new Claim($"{SquidexClaimTypes.UIProperty}:app1:key1", "value1"),
            new Claim($"{SquidexClaimTypes.UIProperty}:app1:key2", "value2 "),
            new Claim($"{SquidexClaimTypes.UIProperty}:app2:key3", "value3"),
        };

        var result = source.GetUIProperties("app1");

        Assert.Equal(new[]
        {
            ("key1", "value1"),
            ("key2", "value2")
        }, result.ToArray());
    }

    [Fact]
    public void Should_extract_ui_claims_v2()
    {
        var source = new[]
        {
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,value"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key1=value1"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key2=value2 "),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app2,key3=value3"),
        };

        var result = source.GetUIProperties("app1");

        Assert.Equal(new[]
        {
            ("key1", "value1"),
            ("key2", "value2")
        }, result.ToArray());
    }
}
