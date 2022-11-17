// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Model;

public class InvariantPartitionTests
{
    [Fact]
    public void Should_provide_name()
    {
        var sut = InvariantPartitioning.Instance;

        Assert.Equal("invariant value", sut.ToString());
    }

    [Fact]
    public void Should_provide_master()
    {
        var sut = InvariantPartitioning.Instance;

        Assert.Equal("iv", sut.Master);
        Assert.Equal("Invariant", sut.GetName("iv"));

        Assert.Equal(new[] { "iv" }, sut.AllKeys.ToArray());
        Assert.Equal(new[] { "iv" }, sut.GetPriorities("iv").ToArray());

        Assert.True(sut.IsMaster("iv"));
        Assert.True(sut.Contains("iv"));

        Assert.False(sut.IsOptional("iv"));
    }

    [Fact]
    public void Should_handle_unsupported_key()
    {
        var sut = InvariantPartitioning.Instance;

        Assert.Null(sut.GetName("invalid"));

        Assert.Empty(sut.GetPriorities("invalid"));

        Assert.False(sut.IsMaster("invalid"));
        Assert.False(sut.IsOptional("invalid"));
        Assert.False(sut.Contains("invalid"));
    }
}
