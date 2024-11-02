// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

public class IndexParserTests
{
    private readonly BsonDocument validSource =
        new BsonDocument
        {
            ["name"] = "custom_index",
            ["key"] = new BsonDocument
            {
                ["mt"] = 1,
                ["mb"] = -1,
                ["do.field1"] = 1,
            }
        };

    static IndexParserTests()
    {
        MongoContentEntity.RegisterClassMap();
    }

    [Fact]
    public void Should_parse_index()
    {
        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.True(result);

        definition.Should().BeEquivalentTo(
            new IndexDefinition()
            {
                new IndexField("lastModified", SortOrder.Ascending),
                new IndexField("lastModifiedBy", SortOrder.Descending),
                new IndexField("data.field1", SortOrder.Ascending),
            });
    }

    [Fact]
    public void Should_not_parse_index_if_prefix_does_not_match()
    {
        var result = IndexParser.TryParse(validSource, "prefix_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }

    [Fact]
    public void Should_not_parse_index_if_name_not_found()
    {
        validSource.Remove("name");

        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }

    [Fact]
    public void Should_not_parse_index_if_name_has_invalid_type()
    {
        validSource["name"] = 42;

        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }

    [Fact]
    public void Should_not_parse_index_if_key_not_found()
    {
        validSource.Remove("key");

        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }

    [Fact]
    public void Should_not_parse_index_if_key_has_invalid_type()
    {
        validSource["key"] = 42;

        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }

    [Fact]
    public void Should_not_parse_index_if_key_is_empty()
    {
        validSource["key"] = new BsonDocument();

        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }

    [Fact]
    public void Should_not_parse_index_if_key_property_has_invalid_type()
    {
        validSource["key"].AsBsonDocument["mt"] = "invalid";

        var result = IndexParser.TryParse(validSource, "custom_", out var definition);

        Assert.False(result);
        Assert.Null(definition);
    }
}
