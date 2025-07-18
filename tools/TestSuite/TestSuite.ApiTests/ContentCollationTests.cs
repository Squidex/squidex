// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

namespace TestSuite.ApiTests;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

public class ContentCollationTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    private readonly string schemaName = $"schema-{Guid.NewGuid()}";

    public CreatedAppFixture _ { get; } = fixture;

    public sealed class SimpleEntityData
    {
        public static readonly string StringField = nameof(String).ToLowerInvariant();

        [JsonConverter(typeof(InvariantConverter))]
        public string? String { get; set; }
    }

    public sealed class SimpleEntity : Content<SimpleEntityData>
    {
    }

    [Fact]
    [Trait("Category", "MongoOnly")]
    public async Task Should_search_based_on_collation()
    {
        // STEP 0: Create schema.
        var schemaRequest = new CreateSchemaDto
        {
            Name = schemaName,
            Fields =
            [
                new UpsertSchemaFieldDto
                {
                    Name = SimpleEntityData.StringField,
                    Properties = new StringFieldPropertiesDto(),
                },
            ],
            IsPublished = true,
        };

        await _.Client.Schemas.PostSchemaAsync(schemaRequest);

        // STEP 1: Create content.
        var contents = _.Client.Contents<SimpleEntity, SimpleEntityData>(schemaName);

        await contents.CreateAsync(
            new SimpleEntityData
            {
                String = "İstanbul",
            },
            ContentCreateOptions.AsPublish);

        await contents.CreateAsync(
            new SimpleEntityData
            {
                String = "Mersin",
            },
            ContentCreateOptions.AsPublish);

        await contents.CreateAsync(
            new SimpleEntityData
            {
                String = "Lüleburgaz",
            },
            ContentCreateOptions.AsPublish);


        // STEP 2: Get sorted contents.
        var sortedContent_1 = await contents.GetAsync(new ContentQuery { OrderBy = $"data/{TestEntityData.StringField}/iv asc" });
        var sortedNames_1 = sortedContent_1.Items.Select(x => x.Data.String).ToList();

        Assert.Equal(new string[] { "Lüleburgaz", "Mersin", "İstanbul" }, sortedNames_1);


        // STEP 3: Get with collation.
        var sortedContent_2 = await contents.GetAsync(new ContentQuery { OrderBy = $"data/{TestEntityData.StringField}/iv asc", Collation = "tr" });
        var sortedNames_2 = sortedContent_2.Items.Select(x => x.Data.String).ToList();

        Assert.Equal(new string[] { "İstanbul", "Lüleburgaz", "Mersin" }, sortedNames_2);
    }
}
