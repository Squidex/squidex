// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public class PagingTests
{
    [Fact]
    public void Should_keep_valid_values()
    {
        var paging = new Paging(30, 10);

        Assert.Equal(30, paging.Skip);
        Assert.Equal(10, paging.Take);
    }

    [Fact]
    public void Should_fix_negative_skip()
    {
        var paging = new Paging(-5, 10);

        Assert.Equal(0, paging.Skip);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_use_default_take_if_take_is_not_positive(int take)
    {
        var paging = new Paging(0, take);

        Assert.Equal(Paging.DefaultTake, paging.Take);
    }

    [Fact]
    public void Should_keep_take_of_one()
    {
        var paging = new Paging(0, 1);

        Assert.Equal(1, paging.Take);
    }

    [Fact]
    public void Should_keep_max_take()
    {
        var paging = new Paging(0, Paging.MaxTake);

        Assert.Equal(Paging.MaxTake, paging.Take);
    }

    [Fact]
    public void Should_limit_too_large_take()
    {
        var paging = new Paging(0, 100_000);

        Assert.Equal(Paging.MaxTake, paging.Take);
    }
}
