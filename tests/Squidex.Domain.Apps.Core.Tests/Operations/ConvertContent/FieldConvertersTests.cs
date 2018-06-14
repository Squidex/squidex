// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class FieldConvertersTests
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.EN, Language.DE);
        private readonly RootField<NumberFieldProperties> numberField = Fields.Number(1, "1", Partitioning.Invariant);
        private readonly RootField<StringFieldProperties> stringLanguageField = Fields.String(1, "1", Partitioning.Language);
        private readonly RootField<StringFieldProperties> stringInvariantField = Fields.String(1, "1", Partitioning.Invariant);
        private readonly RootField<JsonFieldProperties> jsonField = Fields.Json(1, "1", Partitioning.Invariant);
        private readonly RootField<ArrayFieldProperties> arrayField = Fields.Array(1, "1", Partitioning.Invariant,
            Fields.Number(1, "field1"),
            Fields.Number(2, "field2").Hide());

        [Fact]
        public void Should_filter_for_value_conversion()
        {
            var input =
                new ContentFieldData()
                    .AddValue("iv", new JObject());

            var actual = FieldConverters.ForValues((f, i) => Value.Unset)(input, stringInvariantField);

            var expected = new ContentFieldData();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_for_value_conversion()
        {
            var input =
                new ContentFieldData()
                    .AddValue("iv", new JObject());

            var actual = FieldConverters.ForValues(ValueConverters.EncodeJson())(input, jsonField);

            var expected =
                new ContentFieldData()
                    .AddValue("iv", "e30=");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_name_to_id()
        {
            var input =
                new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("field1", 100),
                                new JProperty("field2", 200),
                                new JProperty("invalid", 300))));

            var actual = FieldConverters.ForNestedName2Id(ValueConverters.ExcludeHidden())(input, arrayField);

            var expected =
               new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("1", 100))));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_name_to_name()
        {
            var input =
                new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("field1", 100),
                                new JProperty("field2", 200),
                                new JProperty("invalid", 300))));

            var actual = FieldConverters.ForNestedName2Name(ValueConverters.ExcludeHidden())(input, arrayField);

            var expected =
                new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("field1", 100))));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_id_to_id()
        {
            var input =
               new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("1", 100),
                                new JProperty("2", 200),
                                new JProperty("99", 300))));

            var actual = FieldConverters.ForNestedId2Id(ValueConverters.ExcludeHidden())(input, arrayField);

            var expected =
               new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("1", 100))));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_id_to_name()
        {
            var input =
               new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("1", 100),
                                new JProperty("2", 200),
                                new JProperty("99", 300))));

            var actual = FieldConverters.ForNestedId2Name(ValueConverters.ExcludeHidden())(input, arrayField);

            var expected =
               new ContentFieldData()
                    .AddValue("iv",
                        new JArray(
                            new JObject(
                                new JProperty("field1", 100))));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_return_same_values_if_all_values_are_valid()
        {
            var source =
                new ContentFieldData()
                    .AddValue("en", null)
                    .AddValue("de", 1);

            var result = FieldConverters.ExcludeChangedTypes()(source, numberField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_null_values_any_value_is_invalid()
        {
            var source =
                new ContentFieldData()
                    .AddValue("en", "EN")
                    .AddValue("de", 0);

            var result = FieldConverters.ExcludeChangedTypes()(source, numberField);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_same_values_if_field_not_hidden()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.ExcludeHidden()(source, stringLanguageField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_null_values_if_field_hidden()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.ExcludeHidden()(source, stringLanguageField.Hide());

            Assert.Null(result);
        }

        [Fact]
        public void Should_resolve_languages_and_cleanup_old_languages()
        {
            var source =
                new ContentFieldData()
                    .AddValue("en", "EN")
                    .AddValue("it", "IT");

            var expected =
                new ContentFieldData()
                    .AddValue("en", "EN");

            var result = FieldConverters.ResolveLanguages(languagesConfig)(source, stringLanguageField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_resolve_languages_and_resolve_master_language_from_invariant()
        {
            var source =
                new ContentFieldData()
                    .AddValue("iv", "A")
                    .AddValue("it", "B");

            var expected =
                new ContentFieldData()
                    .AddValue("en", "A");

            var result = FieldConverters.ResolveLanguages(languagesConfig)(source, stringLanguageField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_same_values_if_resolving_languages_from_invariant_field()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.ResolveLanguages(languagesConfig)(source, stringInvariantField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_resolve_invariant_and_use_direct_value()
        {
            var source =
                new ContentFieldData()
                    .AddValue("iv", "A")
                    .AddValue("it", "B");

            var expected =
                new ContentFieldData()
                    .AddValue("iv", "A");

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, stringInvariantField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_resolve_invariant_and_resolve_invariant_from_master_language()
        {
            var source =
                new ContentFieldData()
                    .AddValue("de", "DE")
                    .AddValue("en", "EN");

            var expected =
                new ContentFieldData()
                    .AddValue("iv", "EN");

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, stringInvariantField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_resolve_invariant_and_resolve_invariant_from_first_language()
        {
            var source =
                new ContentFieldData()
                    .AddValue("de", "DE")
                    .AddValue("it", "IT");

            var expected =
                new ContentFieldData()
                    .AddValue("iv", "DE");

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, stringInvariantField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_same_values_if_resolving_invariant_from_language_field()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, stringLanguageField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_language_from_fallback_if_found()
        {
            var config_1 = languagesConfig.Set(new LanguageConfig(Language.IT));
            var config_2 = config_1.Set(new LanguageConfig(Language.ES, false, Language.IT));

            var source =
                new ContentFieldData()
                    .AddValue("en", "EN")
                    .AddValue("it", "IT");

            var expected =
                new ContentFieldData()
                    .AddValue("en", "EN")
                    .AddValue("de", "EN")
                    .AddValue("it", "IT")
                    .AddValue("es", "IT");

            var result = FieldConverters.ResolveFallbackLanguages(config_2)(source, stringLanguageField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_not_return_value_if_master_is_missing()
        {
            var source =
                new ContentFieldData()
                    .AddValue("de", "DE");

            var expected =
                new ContentFieldData()
                    .AddValue("de", "DE");

            var result = FieldConverters.ResolveFallbackLanguages(languagesConfig)(source, stringLanguageField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_filter_languages()
        {
            var source =
                new ContentFieldData()
                    .AddValue("en", "EN")
                    .AddValue("de", "DE");

            var expected =
                new ContentFieldData()
                    .AddValue("de", "DE");

            var result = FieldConverters.FilterLanguages(languagesConfig, new[] { Language.DE })(source, stringLanguageField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_master_language_if_languages_to_filter_are_invalid()
        {
            var source =
                new ContentFieldData()
                    .AddValue("en", "EN")
                    .AddValue("de", "DE");

            var expected =
                new ContentFieldData()
                    .AddValue("en", "EN");

            var result = FieldConverters.FilterLanguages(languagesConfig, new[] { Language.CA })(source, stringLanguageField);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_same_values_if_resolving_fallback_languages_from_invariant_field()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.ResolveFallbackLanguages(languagesConfig)(source, stringInvariantField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_values_if_filtered_languages_is_empty()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.FilterLanguages(languagesConfig, Enumerable.Empty<Language>())(source, stringLanguageField);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_values_if_filtering_languages_from_invariant_field()
        {
            var source = new ContentFieldData();

            var result = FieldConverters.FilterLanguages(languagesConfig, null)(source, stringInvariantField);

            Assert.Same(source, result);
        }
    }
}
