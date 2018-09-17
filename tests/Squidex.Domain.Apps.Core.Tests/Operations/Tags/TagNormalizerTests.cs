// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Tags;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Tags
{
    public class TagNormalizerTests
    {
        private static readonly JTokenEqualityComparer JTokenEqualityComparer = new JTokenEqualityComparer();
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();
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
                A<HashSet<string>>.That.IsSameSequenceAs("n_raw2_1", "n_raw2_2", "n_raw4"),
                A<HashSet<string>>.That.IsSameSequenceAs("o_raw2_1", "o_raw2_2", "o_raw4")))
                .Returns(new Dictionary<string, string>
                {
                    ["n_raw2_2"] = "id2_2",
                    ["n_raw2_1"] = "id2_1",
                    ["n_raw4"] = "id4"
                });

            await tagService.NormalizeAsync(appId, schemaId, schema, newData, oldData);

            Assert.Equal(new JArray("id2_1", "id2_2"), newData["tags2"]["iv"], JTokenEqualityComparer);
            Assert.Equal(new JArray("id4"), newData["array"]["iv"][0]["nestedTags2"], JTokenEqualityComparer);
        }

        [Fact]
        public async Task Should_normalize_tags_without_old_data()
        {
            var newData = GenerateData("name");

            A.CallTo(() => tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId),
                A<HashSet<string>>.That.IsSameSequenceAs("name2_1", "name2_2", "name4"),
                A<HashSet<string>>.That.IsEmpty()))
                .Returns(new Dictionary<string, string>
                {
                    ["name2_2"] = "id2_2",
                    ["name2_1"] = "id2_1",
                    ["name4"] = "id4"
                });

            await tagService.NormalizeAsync(appId, schemaId, schema, newData, null);

            Assert.Equal(new JArray("id2_1", "id2_2"), newData["tags2"]["iv"], JTokenEqualityComparer);
            Assert.Equal(new JArray("id4"), newData["array"]["iv"][0]["nestedTags2"], JTokenEqualityComparer);
        }

        [Fact]
        public async Task Should_denormalize_tags()
        {
            var newData = GenerateData("id");

            A.CallTo(() => tagService.NormalizeTagsAsync(appId, TagGroups.Schemas(schemaId),
                A<HashSet<string>>.That.IsSameSequenceAs("id2_1", "id2_2", "id4"),
                A<HashSet<string>>.That.IsEmpty()))
                .Returns(new Dictionary<string, string>
                {
                    ["id2_2"] = "name2_2",
                    ["id2_1"] = "name2_1",
                    ["id4"] = "name4"
                });

            await tagService.NormalizeAsync(appId, schemaId, schema, newData, null);

            Assert.Equal(new JArray("name2_1", "name2_2"), newData["tags2"]["iv"], JTokenEqualityComparer);
            Assert.Equal(new JArray("name4"), newData["array"]["iv"][0]["nestedTags2"], JTokenEqualityComparer);
        }

        private static NamedContentData GenerateData(string prefix)
        {
            return new NamedContentData()
                .AddField("tags1",
                    new ContentFieldData()
                        .AddValue("iv", new JArray($"{prefix}1")))
                .AddField("tags2",
                    new ContentFieldData()
                        .AddValue("iv", new JArray($"{prefix}2_1", $"{prefix}2_2")))
                .AddField("string",
                    new ContentFieldData()
                        .AddValue("iv", $"{prefix}stringValue"))
                .AddField("array",
                    new ContentFieldData()
                        .AddValue("iv",
                            new JArray(
                                new JObject(
                                    new JProperty("nestedTags1", new JArray($"{prefix}3")),
                                    new JProperty("nestedTags2", new JArray($"{prefix}4")),
                                    new JProperty("string", $"{prefix}nestedStringValue")))));
        }
    }
}
