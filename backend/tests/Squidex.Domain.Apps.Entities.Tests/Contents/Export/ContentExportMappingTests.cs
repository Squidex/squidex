// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Export;

public class ContentExportMappingTests : GivenContext
{
    public ContentExportMappingTests()
    {
        Schema =
            Schema
                .AddString(1, "my-localized", Partitioning.Language)
                .AddNumber(2, "my-number", Partitioning.Invariant)
                .AddUI(3, "my-ui", Partitioning.Invariant);
    }

    [Fact]
    public void Should_parse_fields_with_and_without_names()
    {
        var mapping = ContentExportMapping.Parse("id; Title=data.my-localized.en");

        mapping.Should().BeEquivalentTo(
            new[]
            {
                new { Name = "id", Path = new[] { "id" } },
                new { Name = "Title", Path = new[] { "data", "my-localized", "en" } },
            },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_add_invariant_key_if_data_path_has_no_partition()
    {
        var mapping = ContentExportMapping.Parse("data.my-number");

        mapping.Should().BeEquivalentTo(
            new[]
            {
                new { Name = "data.my-number", Path = new[] { "data", "my-number", "iv" } },
            });
    }

    [Fact]
    public void Should_ignore_format()
    {
        var mapping = ContentExportMapping.Parse("Number=data.my-number/json");

        mapping.Should().BeEquivalentTo(
            new[]
            {
                new { Name = "Number", Path = new[] { "data", "my-number", "iv" } },
            });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" , ")]
    [InlineData("unknown")]
    [InlineData("id.nested")]
    [InlineData("=id")]
    public void Should_throw_exception_if_fields_are_not_valid(string fields)
    {
        Assert.Throws<ValidationException>(() => ContentExportMapping.Parse(fields));
    }

    [Fact]
    public void Should_create_default_mapping_for_csv()
    {
        var mapping = ContentExportMapping.CreateDefault(App, Schema, ExportFormat.Csv);

        mapping.Should().BeEquivalentTo(
            new[]
            {
                new { Name = "id", Path = new[] { "id" } },
                new { Name = "created", Path = new[] { "created" } },
                new { Name = "createdBy", Path = new[] { "createdBy" } },
                new { Name = "lastModified", Path = new[] { "lastModified" } },
                new { Name = "lastModifiedBy", Path = new[] { "lastModifiedBy" } },
                new { Name = "status", Path = new[] { "status" } },
                new { Name = "version", Path = new[] { "version" } },
                new { Name = "data.my-localized.en", Path = new[] { "data", "my-localized", "en" } },
                new { Name = "data.my-localized.de", Path = new[] { "data", "my-localized", "de" } },
                new { Name = "data.my-number.iv", Path = new[] { "data", "my-number", "iv" } },
            },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_create_default_mapping_for_json()
    {
        var mapping = ContentExportMapping.CreateDefault(App, Schema, ExportFormat.Json);

        mapping.Should().BeEquivalentTo(
            new[]
            {
                new { Name = "id", Path = new[] { "id" } },
                new { Name = "created", Path = new[] { "created" } },
                new { Name = "createdBy", Path = new[] { "createdBy" } },
                new { Name = "lastModified", Path = new[] { "lastModified" } },
                new { Name = "lastModifiedBy", Path = new[] { "lastModifiedBy" } },
                new { Name = "status", Path = new[] { "status" } },
                new { Name = "version", Path = new[] { "version" } },
                new { Name = "data", Path = new[] { "data" } },
            },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_get_meta_values()
    {
        var content = CreateContent();

        var mapping = ContentExportMapping.Parse("id,created,createdBy,lastModified,lastModifiedBy,status,newStatus,version");

        var values = mapping.ToDictionary(x => x.Name, x => ContentExportMapping.GetValue(content, x));

        values.Should().BeEquivalentTo(
            new Dictionary<string, JsonValue>
            {
                ["id"] = content.Id.ToString(),
                ["created"] = content.Created,
                ["createdBy"] = User.ToString(),
                ["lastModified"] = content.LastModified,
                ["lastModifiedBy"] = User.ToString(),
                ["status"] = "Published",
                ["newStatus"] = JsonValue.Null,
                ["version"] = 1,
            });
    }

    [Fact]
    public void Should_get_data_values()
    {
        var content = CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("my-localized",
                        new ContentFieldData()
                            .AddLocalized("en", "Hello"))
                    .AddField("my-object",
                        new ContentFieldData()
                            .AddInvariant(new JsonObject().Add("nested", 42))),
        };

        var mapping = ContentExportMapping.Parse("data.my-localized.en,data.my-localized.de,data.my-object.iv.nested,data.unknown,data");

        var values = mapping.ToDictionary(x => x.Name, x => ContentExportMapping.GetValue(content, x));

        values.Should().BeEquivalentTo(
            new Dictionary<string, JsonValue>
            {
                ["data.my-localized.en"] = "Hello",
                ["data.my-localized.de"] = JsonValue.Null,
                ["data.my-object.iv.nested"] = 42,
                ["data.unknown"] = JsonValue.Null,
                ["data"] =
                    new JsonObject()
                        .Add("my-localized",
                            new JsonObject()
                                .Add("en", "Hello"))
                        .Add("my-object",
                            new JsonObject()
                                .Add("iv", new JsonObject().Add("nested", 42))),
            });
    }
}
