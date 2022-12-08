// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class NamesTests
{
    [Fact]
    public void Should_return_name_if_not_taken()
    {
        var sut = ReservedNames.ForFields();

        var result = sut["myName"];

        Assert.Equal("myName", result);
    }

    [Fact]
    public void Should_return_corrected_name_if_not_taken()
    {
        var sut = ReservedNames.ForFields();

        var result = sut["2myName"];

        Assert.Equal("gql_2myName", result);
    }

    [Fact]
    public void Should_return_name_with_offset_if_taken()
    {
        var sut = ReservedNames.ForFields();

        var result1 = sut["myName"];
        var result2 = sut["myName"];
        var result3 = sut["myName"];

        Assert.Equal("myName", result1);
        Assert.Equal("myName2", result2);
        Assert.Equal("myName3", result3);
    }

    [Fact]
    public void Should_return_corrected_name_with_offset_if_taken()
    {
        var sut = ReservedNames.ForFields();

        var result1 = sut["2myName"];
        var result2 = sut["2myName"];
        var result3 = sut["2myName"];

        Assert.Equal("gql_2myName", result1);
        Assert.Equal("gql_2myName2", result2);
        Assert.Equal("gql_2myName3", result3);
    }

    [Fact]
    public void Should_return_name_with_offset_if_reserved()
    {
        var sut = ReservedNames.ForTypes();

        var result = sut["Content"];

        Assert.Equal("Content2", result);
    }
}
