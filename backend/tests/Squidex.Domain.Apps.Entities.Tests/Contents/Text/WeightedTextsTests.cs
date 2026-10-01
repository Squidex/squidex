// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class WeightedTextsTests
{
    [Fact]
    public void Should_return_texts_if_no_titles_defined()
    {
        var texts = new Dictionary<string, string> { ["en"] = "World" };

        var actual = new UpsertIndexEntry { Texts = texts }.GetWeightedTexts();

        Assert.Same(texts, actual);
    }

    [Fact]
    public void Should_return_null_if_nothing_defined()
    {
        var actual = new UpsertIndexEntry().GetWeightedTexts();

        Assert.Null(actual);
    }

    [Fact]
    public void Should_repeat_titles_before_texts()
    {
        var upsert = new UpsertIndexEntry
        {
            Texts = new Dictionary<string, string>
            {
                ["en"] = "World",
                ["de"] = "Welt",
            },
            Titles = new Dictionary<string, string>
            {
                ["en"] = "Hello",
                ["iv"] = "Title",
            },
        };

        var actual = upsert.GetWeightedTexts();

        actual.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["en"] = "Hello Hello Hello World",
                ["de"] = "Welt",
                ["iv"] = "Title Title Title",
            });
    }

    [Fact]
    public void Should_not_modify_texts_of_upsert()
    {
        var upsert = new UpsertIndexEntry
        {
            Texts = new Dictionary<string, string> { ["en"] = "World" },
            Titles = new Dictionary<string, string> { ["en"] = "Hello" },
        };

        upsert.GetWeightedTexts();

        Assert.Equal("World", upsert.Texts["en"]);
    }
}
