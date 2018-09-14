// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class ValueConvertersTests
    {
        private readonly RootField<StringFieldProperties> stringField = Fields.String(1, "1", Partitioning.Invariant);
        private readonly RootField<JsonFieldProperties> jsonField = Fields.Json(1, "1", Partitioning.Invariant);
        private readonly RootField<NumberFieldProperties> numberField = Fields.Number(1, "1", Partitioning.Invariant);

        [Fact]
        public void Should_encode_json_value()
        {
            var source = new JObject();

            var result = ValueConverters.EncodeJson()(source, jsonField);

            Assert.Equal("e30=", result);
        }

        [Fact]
        public void Should_return_same_value_if_encoding_null_value()
        {
            var source = JValue.CreateNull();

            var result = ValueConverters.EncodeJson()(source, jsonField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_value_if_encoding_non_json_field()
        {
            var source = (JToken)"NO-JSON";

            var result = ValueConverters.EncodeJson()(source, stringField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_decode_json_values()
        {
            var source = "e30=";

            var result = ValueConverters.DecodeJson()(source, jsonField);

            Assert.Equal(new JObject(), result);
        }

        [Fact]
        public void Should_return_same_value_if_decoding_null_value()
        {
            var source = JValue.CreateNull();

            var result = ValueConverters.DecodeJson()(source, jsonField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_value_if_decoding_non_json_field()
        {
            var source = JValue.CreateNull();

            var result = ValueConverters.EncodeJson()(source, stringField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_unset_if_field_hidden()
        {
            var source = 123;

            var result = ValueConverters.ExcludeHidden()(source, stringField.Hide());

            Assert.Same(Value.Unset, result);
        }

        [Fact]
        public void Should_return_unset_if_field_has_wrong_type()
        {
            var source = "invalid";

            var result = ValueConverters.ExcludeChangedTypes()(source, numberField);

            Assert.Same(Value.Unset, result);
        }
    }
}
