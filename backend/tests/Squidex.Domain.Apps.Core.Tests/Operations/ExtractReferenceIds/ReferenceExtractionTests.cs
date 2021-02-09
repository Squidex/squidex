// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ExtractReferenceIds
{
    public class ReferenceExtractionTests
    {
        private readonly Schema schema;

        public ReferenceExtractionTests()
        {
            schema =
                new Schema("my-schema")
                    .AddReferences(1, "references", Partitioning.Invariant)
                    .AddAssets(2, "assets", Partitioning.Invariant)
                    .AddArray(3, "array", Partitioning.Invariant, a => a
                        .AddAssets(31, "nested"));
        }

        [Fact]
        public void Should_get_ids_from_name_data()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var input =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(id1.ToString(), id2.ToString())));

            var ids = new HashSet<DomainId>();

            input.AddReferencedIds(schema, ids);

            Assert.Equal(new[] { id1, id2 }, ids);
        }

        [Fact]
        public void Should_get_limited_ids_from_name_data()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var input =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(id1.ToString(), id2.ToString())));

            var ids = new HashSet<DomainId>();

            input.AddReferencedIds(schema, ids, 1);

            Assert.Equal(new[] { id1 }, ids);
        }

        [Fact]
        public void Should_cleanup_deleted_ids()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var source =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(id1, id2)))
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(id1)))
                    .AddField("array",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("nested", JsonValue.Array(id1, id2)))));

            var expected =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(id2)))
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array()))
                    .AddField("array",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("nested", JsonValue.Array(id2)))));

            var cleaner = ValueReferencesConverter.CleanReferences(new HashSet<DomainId> { id2 });
            var cleanNested = ValueConverters.ForNested(cleaner);

            var converter = FieldConverters.ForValues(cleaner, cleanNested);

            var actual = source.Convert(schema, converter);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_return_empty_list_from_non_references_field()
        {
            var sut = Fields.String(1, "my-string", Partitioning.Invariant);

            var result = sut.GetReferencedIds(JsonValue.Create("invalid")).ToArray();

            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(ReferencingNestedFields))]
        public void Should_return_ids_from_nested_field(NestedField field)
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var arrayField = Fields.Array(1, "my-array", Partitioning.Invariant, field);

            var value =
                JsonValue.Array(
                    JsonValue.Object()
                        .Add(field.Name, CreateValue(id1, id2)));

            var result = arrayField.GetReferencedIds(value).ToArray();

            Assert.Equal(new[] { id1, id2 }, result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_return_empty_list_from_field_when_value_item_is_invalid(IField field)
        {
            var result = field.GetReferencedIds(JsonValue.Array(1)).ToArray();

            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_return_empty_list_from_field_when_value_is_empty(IField field)
        {
            var result = field.GetReferencedIds(JsonValue.Array()).ToArray();

            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_return_empty_list_from_field_when_value_is_json_null(IField field)
        {
            var result = field.GetReferencedIds(null).ToArray();

            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_return_empty_list_from_field_when_value_is_null(IField field)
        {
            var result = field.GetReferencedIds(null).ToArray();

            Assert.Empty(result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_return_ids_from_field(IField field)
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var value = CreateValue(id1, id2);

            var result = field.GetReferencedIds(value);

            Assert.Equal(new HashSet<DomainId> { id1, id2 }, result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_return_same_value_from_field_when_value_is_json_null(IField field)
        {
            var result = ValueReferencesConverter.CleanReferences(RandomIds())(JsonValue.Null, field, null);

            Assert.Equal(JsonValue.Null, result);
        }

        [Theory]
        [MemberData(nameof(ReferencingFields))]
        public void Should_remove_deleted_ids_from_field(IField field)
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var value = CreateValue(id1, id2);

            var result = ValueReferencesConverter.CleanReferences(HashSet.Of(id1))(value, field, null);

            Assert.Equal(CreateValue(id1), result);
        }

        public static IEnumerable<object[]> ReferencingNestedFields()
        {
            yield return new object[] { Fields.References(1, "my-refs") };
            yield return new object[] { Fields.Assets(1, "my-assets") };
        }

        public static IEnumerable<object[]> ReferencingFields()
        {
            yield return new object[] { Fields.References(1, "my-refs", Partitioning.Invariant) };
            yield return new object[] { Fields.Assets(1, "my-assets", Partitioning.Invariant) };
        }

        private static HashSet<DomainId> RandomIds()
        {
            return HashSet.Of(DomainId.NewGuid());
        }

        private static IJsonValue CreateValue(params object[] ids)
        {
            return JsonValue.Array(ids);
        }
    }
}
