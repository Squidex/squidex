// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public class StreamFilterTests
{
    [Fact]
    public void Should_simplify_input_to_default_filter()
    {
        var sut = new StreamFilter(StreamFilterKind.MatchFull);

        Assert.Equal(default, sut);
    }

    [Fact]
    public void Should_simplify_input_to_default_filter_with_factory()
    {
        var sut = StreamFilter.Name();

        Assert.Equal(default, sut);
    }
}
