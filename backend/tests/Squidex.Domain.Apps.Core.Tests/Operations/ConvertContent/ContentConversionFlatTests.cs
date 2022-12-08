// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class ContentConversionFlatTests
{
    private readonly ContentData source =
        new ContentData()
            .AddField("field1",
                new ContentFieldData()
                    .AddLocalized("de", 1)
                    .AddLocalized("en", 2))
            .AddField("field2",
                new ContentFieldData()
                    .AddLocalized("de", JsonValue.Null)
                    .AddLocalized("it", 4))
            .AddField("field3",
                new ContentFieldData()
                    .AddLocalized("en", 6))
            .AddField("field4",
                new ContentFieldData()
                    .AddLocalized("it", 7))
            .AddField("field5",
                new ContentFieldData());

    [Fact]
    public void Should_return_flatten_value()
    {
        var output = source.ToFlatten();

        var expected = new Dictionary<string, object?>
        {
            {
                "field1",
                new ContentFieldData()
                    .AddLocalized("de", 1)
                    .AddLocalized("en", 2)
            },
            {
                "field2",
                new ContentFieldData()
                    .AddLocalized("de", JsonValue.Null)
                    .AddLocalized("it", 4)
            },
            { "field3", JsonValue.Create(6) },
            { "field4", JsonValue.Create(7) }
        };

        Assert.True(expected.EqualsDictionary(output));
    }

    [Fact]
    public void Should_return_flatten_value_and_always_with_first()
    {
        var output = source.ToFlatten("it");

        var expected = new FlatContentData
        {
            { "field1", JsonValue.Create(1) },
            { "field2", JsonValue.Create(4) },
            { "field3", JsonValue.Create(6) },
            { "field4", JsonValue.Create(7) }
        };

        Assert.True(expected.EqualsDictionary(output));
    }
}
