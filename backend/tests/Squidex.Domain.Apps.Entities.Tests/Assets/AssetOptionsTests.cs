// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetOptionsTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(100, null)]
    [InlineData(null, 100)]
    [InlineData(4096, 4096)]
    public void Should_accept_resize_size_within_limit(int? width, int? height)
    {
        var sut = new AssetOptions { MaxResizeDimension = 4096 };

        Assert.True(sut.IsValidResizeSize(width, height));
    }

    [Theory]
    [InlineData(4097, null)]
    [InlineData(null, 4097)]
    [InlineData(100, 50000)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Should_reject_resize_size_above_limit(int? width, int? height)
    {
        var sut = new AssetOptions { MaxResizeDimension = 4096 };

        Assert.False(sut.IsValidResizeSize(width, height));
    }

    [Fact]
    public void Should_accept_any_resize_size_if_limit_disabled()
    {
        var sut = new AssetOptions { MaxResizeDimension = 0 };

        Assert.True(sut.IsValidResizeSize(int.MaxValue, int.MaxValue));
    }
}
