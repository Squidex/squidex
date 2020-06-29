// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class FilterTagTransformerTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly ISchemaEntity schema;
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

        public FilterTagTransformerTests()
        {
            var schemaDef =
                new Schema("schema")
                    .AddTags(1, "tags1", Partitioning.Invariant)
                    .AddTags(2, "tags2", Partitioning.Invariant, new TagsFieldProperties { Normalization = TagsFieldNormalization.Schema })
                    .AddString(3, "string", Partitioning.Invariant);

            schema = Mocks.Schema(appId, schemaId, schemaDef);
        }

        [Fact]
        public void Should_normalize_tags()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId.Id, TagGroups.Schemas(schemaId.Id), A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string> { ["name1"] = "id1" });

            var source = ClrFilter.Eq("data.tags2.iv", "name1");

            var result = FilterTagTransformer.TransformAsync(source, appId.Id, schema, tagService);

            Assert.Equal("data.tags2.iv == 'id1'", result!.ToString());
        }

        [Fact]
        public void Should_not_fail_when_tags_not_found()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string>());

            var source = ClrFilter.Eq("data.tags2.iv", "name1");

            var result = FilterTagTransformer.TransformAsync(source, appId.Id, schema, tagService);

            Assert.Equal("data.tags2.iv == 'name1'", result!.ToString());
        }

        [Fact]
        public void Should_not_normalize_other_tags_field()
        {
            var source = ClrFilter.Eq("data.tags1.iv", "value");

            var result = FilterTagTransformer.TransformAsync(source, appId.Id, schema, tagService);

            Assert.Equal("data.tags1.iv == 'value'", result!.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId.Id, A<string>._, A<HashSet<string>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_normalize_other_typed_field()
        {
            var source = ClrFilter.Eq("data.string.iv", "value");

            var result = FilterTagTransformer.TransformAsync(source, appId.Id, schema, tagService);

            Assert.Equal("data.string.iv == 'value'", result!.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId.Id, A<string>._, A<HashSet<string>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_normalize_non_data_field()
        {
            var source = ClrFilter.Eq("no.data", "value");

            var result = FilterTagTransformer.TransformAsync(source, appId.Id, schema, tagService);

            Assert.Equal("no.data == 'value'", result!.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId.Id, A<string>._, A<HashSet<string>>._))
                .MustNotHaveHappened();
        }
    }
}
