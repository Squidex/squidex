// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Tags
{
    public class TagNormalizerTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId schemaId = DomainId.NewGuid();
        private readonly Schema schema;

        public TagNormalizerTests()
        {
            schema =
                new Schema("my-schema")
                    .AddTags(1, "tags1", Partitioning.Invariant)
                    .AddTags(2, "tags2", Partitioning.Invariant, new TagsFieldProperties { Normalization = TagsFieldNormalization.Schema })
                    .AddString(3, "string", Partitioning.Invariant)
                    .AddArray(4, "array", Partitioning.Invariant, f => f
                        .AddTags(401, "nestedTags1")
                        .AddTags(402, "nestedTags2", new TagsFieldProperties { Normalization = TagsFieldNormalization.Schema })
                        .AddString(403, "string"));
        }

        [Fact]
        public async Task Should_normalize_tags_with_old_data()
        {
            var newData = GenerateData("n_raw");
            var oldData = GenerateData("o_raw");

            A.CallTo(() => tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId),
                A<HashSet<string>>.That.Is("n_raw2_1", "n_raw2_2", "n_raw4"),
                A<HashSet<string>>.That.Is("o_raw2_1", "o_raw2_2", "o_raw4")))
                .Returns(new Dictionary<string, string>
                {
                    ["n_raw2_2"] = "id2_2",
                    ["n_raw2_1"] = "id2_1",
                    ["n_raw4"] = "id4"
                });

            await tagService.NormalizeAsync(appId, schemaId, schema, newData, oldData);

            Assert.Equal(JsonValue.Array("id2_1", "id2_2"), newData["tags2"]!["iv"]);
            Assert.Equal(JsonValue.Array("id4"), GetNestedTags(newData));
        }

        [Fact]
        public async Task Should_normalize_tags_without_old_data()
        {
            var newData = GenerateData("name");

            A.CallTo(() => tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId),
                A<HashSet<string>>.That.Is("name2_1", "name2_2", "name4"),
                A<HashSet<string>>.That.IsEmpty()))
                .Returns(new Dictionary<string, string>
                {
                    ["name2_2"] = "id2_2",
                    ["name2_1"] = "id2_1",
                    ["name4"] = "id4"
                });

            await tagService.NormalizeAsync(appId, schemaId, schema, newData, null);

            Assert.Equal(JsonValue.Array("id2_1", "id2_2"), newData["tags2"]!["iv"]);
            Assert.Equal(JsonValue.Array("id4"), GetNestedTags(newData));
        }

        [Fact]
        public async Task Should_denormalize_tags()
        {
            var newData = GenerateData("id");

            A.CallTo(() => tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId),
                A<HashSet<string>>.That.Is("id2_1", "id2_2", "id4"),
                A<HashSet<string>>.That.IsEmpty()))
                .Returns(new Dictionary<string, string>
                {
                    ["id2_2"] = "name2_2",
                    ["id2_1"] = "name2_1",
                    ["id4"] = "name4"
                });

            await tagService.NormalizeAsync(appId, schemaId, schema, newData, null);

            Assert.Equal(JsonValue.Array("name2_1", "name2_2"), newData["tags2"]!["iv"]);
            Assert.Equal(JsonValue.Array("name4"), GetNestedTags(newData));
        }

        private static IJsonValue GetNestedTags(ContentData newData)
        {
            var array = (JsonArray)newData["array"]!["iv"];
            var arrayItem = (JsonObject)array[0];

            return arrayItem["nestedTags2"];
        }

        private static ContentData GenerateData(string prefix)
        {
            return new ContentData()
                .AddField("tags1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array($"{prefix}1")))
                .AddField("tags2",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array($"{prefix}2_1", $"{prefix}2_2")))
                .AddField("string",
                    new ContentFieldData()
                        .AddInvariant($"{prefix}stringValue"))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nestedTags1", JsonValue.Array($"{prefix}3"))
                                    .Add("nestedTags2", JsonValue.Array($"{prefix}4"))
                                    .Add("string", $"{prefix}nestedStringValue"))));
        }
    }
}
