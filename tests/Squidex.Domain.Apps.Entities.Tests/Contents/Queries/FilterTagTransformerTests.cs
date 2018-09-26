// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class FilterTagTransformerTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid schemaId = Guid.NewGuid();

        public FilterTagTransformerTests()
        {
            var schemaDef =
                new Schema("schema")
                    .AddTags(1, "tags1", Partitioning.Invariant)
                    .AddTags(2, "tags2", Partitioning.Invariant, new TagsFieldProperties { Normalization = TagsFieldNormalization.Schema })
                    .AddString(3, "string", Partitioning.Invariant);

            A.CallTo(() => schema.Id).Returns(schemaId);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);
        }

        [Fact]
        public void Should_normalize_tags()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId, TagGroups.Schemas(schemaId), A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string> { ["name1"] = "id1" });

            var source = FilterBuilder.Eq("data.tags2.iv", "name1");
            var result = FilterTagTransformer.Transform(source, appId, schema, tagService);

            Assert.Equal("data.tags2.iv == 'id1'", result.ToString());
        }

        [Fact]
        public void Should_not_fail_when_tags_not_found()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId, TagGroups.Assets, A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string>());

            var source = FilterBuilder.Eq("data.tags2.iv", "name1");
            var result = FilterTagTransformer.Transform(source, appId, schema, tagService);

            Assert.Equal("data.tags2.iv == 'name1'", result.ToString());
        }

        [Fact]
        public void Should_not_normalize_other_tags_field()
        {
            var source = FilterBuilder.Eq("data.tags1.iv", "value");
            var result = FilterTagTransformer.Transform(source, appId, schema, tagService);

            Assert.Equal("data.tags1.iv == 'value'", result.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId, A<string>.Ignored, A<HashSet<string>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_normalize_other_typed_field()
        {
            var source = FilterBuilder.Eq("data.string.iv", "value");
            var result = FilterTagTransformer.Transform(source, appId, schema, tagService);

            Assert.Equal("data.string.iv == 'value'", result.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId, A<string>.Ignored, A<HashSet<string>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_normalize_non_data_field()
        {
            var source = FilterBuilder.Eq("no.data", "value");
            var result = FilterTagTransformer.Transform(source, appId, schema, tagService);

            Assert.Equal("no.data == 'value'", result.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId, A<string>.Ignored, A<HashSet<string>>.Ignored))
                .MustNotHaveHappened();
        }
    }
}
