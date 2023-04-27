// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Infrastructure.Json.Objects;
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
            ("key1", JsonValue.Create("value1")),
            ("key2", JsonValue.Create("value2"))
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
            ("key1", JsonValue.Create("value1")),
            ("key2", JsonValue.Create("value2"))
        }, result.ToArray());
    }

    [Fact]
    public void Should_extract_and_parse_values()
    {
        var source = new[]
        {
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key1=null"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key2=true"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key3=false"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key4=42"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key5=42.5"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key6=string1"),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key7=\"string2\""),
            new Claim($"{SquidexClaimTypes.UIProperty}", "app1,key8=\"string3\" "),
        };

        var result = source.GetUIProperties("app1");

        Assert.Equal(new[]
        {
            ("key1", JsonValue.Null),
            ("key2", JsonValue.True),
            ("key3", JsonValue.False),
            ("key4", JsonValue.Create(42)),
            ("key5", JsonValue.Create(42.5)),
            ("key6", JsonValue.Create("string1")),
            ("key7", JsonValue.Create("string2")),
            ("key8", JsonValue.Create("string3")),
        }, result.ToArray());
    }
}
