// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace Squidex.Domain.Apps.Core.Operations.ExtractReferenceIds
{
    public class ReferenceExtractionTests
    {
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Schema schema;

        public ReferenceExtractionTests()
        {
            schema =
                new Schema("my-schema")
                    .AddNumber(1, "field1", Partitioning.Language)
                    .AddNumber(2, "field2", Partitioning.Invariant)
                    .AddNumber(3, "field3", Partitioning.Invariant)
                    .AddAssets(5, "assets1", Partitioning.Invariant)
                    .AddAssets(6, "assets2", Partitioning.Invariant)
                    .AddArray(7, "array", Partitioning.Invariant, a => a
                        .AddAssets(71, "assets71"))
                    .AddJson(4, "json", Partitioning.Language)
                    .UpdateField(3, f => f.Hide());
        }

        [Fact]
        public void Should_remove_ids()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var input =
                new IdContentData()
                    .AddField(5,
                        new ContentFieldData()
                            .AddValue("iv", new JArray(id1.ToString(), id2.ToString())));

            var ids = input.GetReferencedIds(schema).ToArray();

            Assert.Equal(new[] { id1, id2 }, ids);
        }

        [Fact]
        public void Should_cleanup_deleted_ids()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var input =
                new IdContentData()
                    .AddField(5,
                        new ContentFieldData()
                            .AddValue("iv", new JArray(id1.ToString(), id2.ToString())));

            var converter = FieldConverters.ForValues(ValueReferencesConverter.CleanReferences(new[] { id2 }));

            var actual = input.ConvertId2Id(schema, converter);

            var cleanedValue = (JArray)actual[5]["iv"];

            Assert.Equal(1, cleanedValue.Count);
            Assert.Equal(id1.ToString(), cleanedValue[0]);
        }

        [Fact]
        public void Should_return_ids_from_assets_field()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.Assets(1, "my-asset", Partitioning.Invariant);

            var result = sut.ExtractReferences(CreateValue(id1, id2)).ToArray();

            Assert.Equal(new[] { id1, id2 }, result);
        }

        [Fact]
        public void Should_return_empty_list_from_assets_field_for_referenced_ids_when_null()
        {
            var sut = Fields.Assets(1, "my-asset", Partitioning.Invariant);

            var result = sut.ExtractReferences(null).ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_return_empty_list_from_assets_field_for_referenced_ids_when_other_type()
        {
            var sut = Fields.Assets(1, "my-asset", Partitioning.Invariant);

            var result = sut.ExtractReferences("invalid").ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_return_empty_list_from_non_references_field()
        {
            var sut = Fields.String(1, "my-string", Partitioning.Invariant);

            var result = sut.ExtractReferences("invalid").ToArray();

            Assert.Empty(result);
        }

        [Fact]
        public void Should_return_null_from_assets_field_when_removing_references_from_null_array()
        {
            var sut = Fields.Assets(1, "my-asset", Partitioning.Invariant);

            var result = sut.CleanReferences(null, null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_remove_deleted_references_from_assets_field()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.Assets(1, "my-asset", Partitioning.Invariant);

            var result = sut.CleanReferences(CreateValue(id1, id2), HashSet.Of(id2));

            Assert.Equal(CreateValue(id1), result);
        }

        [Fact]
        public void Should_return_same_token_from_assets_field_when_removing_references_and_nothing_to_remove()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.Assets(1, "my-asset", Partitioning.Invariant);

            var token = CreateValue(id1, id2);
            var result = sut.CleanReferences(token, HashSet.Of(Guid.NewGuid()));

            Assert.Same(token, result);
        }

        [Fact]
        public void Should_return_ids_from_nested_references_field()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut =
                Fields.Array(1, "my-array", Partitioning.Invariant,
                    Fields.References(1, "my-refs",
                        new ReferencesFieldProperties { SchemaId = schemaId }));

            var value =
                new JArray(
                    new JObject(
                        new JProperty("my-refs", CreateValue(id1, id2))));

            var result = sut.ExtractReferences(value).ToArray();

            Assert.Equal(new[] { id1, id2, schemaId }, result);
        }

        [Fact]
        public void Should_return_ids_from_references_field()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.References(1, "my-refs", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = schemaId });

            var result = sut.ExtractReferences(CreateValue(id1, id2)).ToArray();

            Assert.Equal(new[] { id1, id2, schemaId }, result);
        }

        [Fact]
        public void Should_return_list_from_references_field_with_schema_id_list_for_referenced_ids_when_null()
        {
            var sut = Fields.References(1, "my-refs", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = schemaId });

            var result = sut.ExtractReferences(null).ToArray();

            Assert.Equal(new[] { schemaId }, result);
        }

        [Fact]
        public void Should_return_list_from_references_field_with_schema_id_for_referenced_ids_when_other_type()
        {
            var sut = Fields.References(1, "my-refs", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = schemaId });

            var result = sut.ExtractReferences("invalid").ToArray();

            Assert.Equal(new[] { schemaId }, result);
        }

        [Fact]
        public void Should_return_null_from_references_field_when_removing_references_from_null_array()
        {
            var sut = Fields.References(1, "my-refs", Partitioning.Invariant);

            var result = sut.CleanReferences(null, null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_remove_deleted_references_from_references_field()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.References(1, "my-refs", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = schemaId });

            var result = sut.CleanReferences(CreateValue(id1, id2), HashSet.Of(id2));

            Assert.Equal(CreateValue(id1), result);
        }

        [Fact]
        public void Should_remove_all_references_from_references_field_when_schema_is_removed()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.References(1, "my-refs", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = schemaId });

            var result = sut.CleanReferences(CreateValue(id1, id2), HashSet.Of(schemaId));

            Assert.Equal(CreateValue(), result);
        }

        [Fact]
        public void Should_return_same_token_from_references_field_when_removing_references_and_nothing_to_remove()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var sut = Fields.References(1, "my-refs", Partitioning.Invariant);

            var token = CreateValue(id1, id2);
            var result = sut.CleanReferences(token, HashSet.Of(Guid.NewGuid()));

            Assert.Same(token, result);
        }

        private static JToken CreateValue(params Guid[] ids)
        {
            return ids == null ? JValue.CreateNull() : (JToken)new JArray(ids.OfType<object>().ToArray());
        }
    }
}
