// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ValueConvertersTests
    {
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly Guid id1 = Guid.NewGuid();
        private readonly Guid id2 = Guid.NewGuid();
        private readonly RootField<StringFieldProperties> stringField = Fields.String(1, "1", Partitioning.Invariant);
        private readonly RootField<JsonFieldProperties> jsonField = Fields.Json(1, "1", Partitioning.Invariant);
        private readonly RootField<NumberFieldProperties> numberField = Fields.Number(1, "1", Partitioning.Invariant);

        public ValueConvertersTests()
        {
            A.CallTo(() => urlGenerator.AssetContent(A<Guid>._))
                .ReturnsLazily(ctx => $"url/to/{ctx.GetArgument<Guid>(0)}");
        }

        [Fact]
        public void Should_encode_json_value()
        {
            var source = JsonValue.Object();

            var result = ValueConverters.EncodeJson(TestUtils.DefaultSerializer)(source, jsonField);

            Assert.Equal(JsonValue.Create("e30="), result);
        }

        [Fact]
        public void Should_return_same_value_if_encoding_null_value()
        {
            var source = JsonValue.Null;

            var result = ValueConverters.EncodeJson(TestUtils.DefaultSerializer)(source, jsonField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_value_if_encoding_non_json_field()
        {
            var source = JsonValue.Create("NO-JSON");

            var result = ValueConverters.EncodeJson(TestUtils.DefaultSerializer)(source, stringField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_decode_json_values()
        {
            var source = JsonValue.Create("e30=");

            var result = ValueConverters.DecodeJson(TestUtils.DefaultSerializer)(source, jsonField);

            Assert.Equal(JsonValue.Object(), result);
        }

        [Fact]
        public void Should_return_same_value_if_decoding_null_value()
        {
            var source = JsonValue.Null;

            var result = ValueConverters.DecodeJson(TestUtils.DefaultSerializer)(source, jsonField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_value_if_decoding_non_json_field()
        {
            var source = JsonValue.Null;

            var result = ValueConverters.EncodeJson(TestUtils.DefaultSerializer)(source, stringField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_null_if_field_hidden()
        {
            var source = JsonValue.Create(123);

            var result = ValueConverters.ExcludeHidden(source, stringField.Hide());

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_if_field_has_wrong_type()
        {
            var source = JsonValue.Create("invalid");

            var result = ValueConverters.ExcludeChangedTypes(source, numberField);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("assets")]
        [InlineData("*")]
        public void Should_convert_asset_ids_to_urls(string path)
        {
            var field = Fields.Assets(1, "assets", Partitioning.Invariant);

            var source = JsonValue.Array(id1, id2);

            var expected = JsonValue.Array($"url/to/{id1}", $"url/to/{id2}");

            var result = ValueConverters.ResolveAssetUrls(HashSet.Of(path), urlGenerator)(source, field);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("other")]
        [InlineData("**")]
        public void Should_not_convert_asset_ids_when_field_name_does_not_match(string path)
        {
            var field = Fields.Assets(1, "assets", Partitioning.Invariant);

            var source = JsonValue.Array(id1, id2);

            var expected = source;

            var result = ValueConverters.ResolveAssetUrls(HashSet.Of(path), urlGenerator)(source, field);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("parent.assets")]
        [InlineData("*")]
        public void Should_convert_nested_asset_ids_to_urls(string path)
        {
            var field = Fields.Array(1, "parent", Partitioning.Invariant, Fields.Assets(11, "assets"));

            var source = JsonValue.Array(id1, id2);

            var expected = JsonValue.Array($"url/to/{id1}", $"url/to/{id2}");

            var result = ValueConverters.ResolveAssetUrls(HashSet.Of(path), urlGenerator)(source, field.Fields[0], field);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("assets")]
        [InlineData("parent")]
        [InlineData("parent.other")]
        [InlineData("other.assets")]
        public void Should_not_convert_nested_asset_ids_when_field_name_does_not_match(string path)
        {
            var field = Fields.Array(1, "parent", Partitioning.Invariant, Fields.Assets(11, "assets"));

            var source = JsonValue.Array(id1, id2);

            var expected = source;

            var result = ValueConverters.ResolveAssetUrls(HashSet.Of(path), urlGenerator)(source, field.Fields[0], field);

            Assert.Equal(expected, result);
        }
    }
}
