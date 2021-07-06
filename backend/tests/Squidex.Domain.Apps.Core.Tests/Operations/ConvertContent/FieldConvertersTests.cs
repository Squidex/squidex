// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent
{
    public class FieldConvertersTests
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.English.Set(Language.DE);

        private static IEnumerable<object?[]> InvalidValues()
        {
            yield return new object?[] { null };
            yield return new object?[] { JsonValue.Null };
            yield return new object?[] { JsonValue.False }; // Undefined
        }

        [Fact]
        public void Should_convert_data_with_value_converter()
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source =
                new ContentFieldData()
                    .AddInvariant(JsonValue.Object());

            var result = FieldConverters.ForValues(ResolvedComponents.Empty, (value, field, parent) => null)(source, field);

            var expected = new ContentFieldData();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_field_data_if_excluding_changed_types_and_all_values_are_valid()
        {
            var field = Fields.Number(1, "number", Partitioning.Invariant);

            var source =
                new ContentFieldData()
                    .AddLocalized("en", null)
                    .AddLocalized("de", 1);

            var result = FieldConverters.ExcludeChangedTypes(TestUtils.DefaultSerializer)(source, field);

            Assert.Equal(source, result);
        }

        [Fact]
        public void Should_return_null_if_excluding_changed_types_and_one_value_is_invalid()
        {
            var field = Fields.Number(1, "number", Partitioning.Invariant);

            var source =
                new ContentFieldData()
                    .AddLocalized("en", "EN")
                    .AddLocalized("de", 0);

            var result = FieldConverters.ExcludeChangedTypes(TestUtils.DefaultSerializer)(source, field);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_field_data_if_field_not_hidden()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source = new ContentFieldData();

            var result = FieldConverters.ExcludeHidden(source, field);

            Assert.Equal(source, result);
        }

        [Fact]
        public void Should_return_null_if_field_hidden()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source = new ContentFieldData();

            var result = FieldConverters.ExcludeHidden(source, field.Hide());

            Assert.Null(result);
        }

        [Fact]
        public void Should_resolve_languages_and_cleanup_old_languages()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source =
                new ContentFieldData()
                    .AddLocalized("en", "EN")
                    .AddLocalized("it", "IT");

            var expected =
                new ContentFieldData()
                    .AddLocalized("en", "EN");

            var result = FieldConverters.ResolveLanguages(languagesConfig)(source, field);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(InvalidValues))]
        public void Should_resolve_master_language_from_invariant(IJsonValue? value)
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source =
                new ContentFieldData()
                    .AddLocalized("iv", "A")
                    .AddLocalized("it", "B");

            if (value != JsonValue.False)
            {
                source["en"] = value!;
            }

            var expected =
                new ContentFieldData()
                    .AddLocalized("en", "A");

            var result = FieldConverters.ResolveLanguages(languagesConfig)(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_not_resolve_master_language_if_not_found()
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source = new ContentFieldData();

            var result = FieldConverters.ResolveLanguages(languagesConfig)(source, field);

            Assert.Equal(source, result);
        }

        [Fact]
        public void Should_resolve_invariant()
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source =
                new ContentFieldData()
                    .AddInvariant("A");

            var expected =
                new ContentFieldData()
                    .AddInvariant("A");

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, field);

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(InvalidValues))]
        public void Should_resolve_invariant_from_master_language(IJsonValue? value)
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source =
                new ContentFieldData()
                    .AddLocalized("de", "DE")
                    .AddLocalized("en", "EN");

            if (value != JsonValue.False)
            {
                source[InvariantPartitioning.Key] = value!;
            }

            var expected =
                new ContentFieldData()
                    .AddInvariant("EN");

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_resolve_invariant_from_first_language()
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source =
                new ContentFieldData()
                    .AddLocalized("de", "DE")
                    .AddLocalized("it", "IT");

            var expected =
                new ContentFieldData()
                    .AddInvariant("DE");

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_not_resolve_invariant_if_not_found()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source = new ContentFieldData();

            var result = FieldConverters.ResolveInvariant(languagesConfig)(source, field);

            Assert.Same(source, result);
        }

        [Theory]
        [MemberData(nameof(InvalidValues))]
        public void Should_resolve_from_fallback_language_if_found(IJsonValue? value)
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var config =
                LanguagesConfig.English
                    .Set(Language.DE)
                    .Set(Language.IT)
                    .Set(Language.ES, false, Language.IT);

            var source =
                new ContentFieldData()
                    .AddLocalized("en", "EN")
                    .AddLocalized("it", "IT");

            if (value != JsonValue.False)
            {
                source["de"] = value!;
            }

            var expected =
                new ContentFieldData()
                    .AddLocalized("en", "EN")
                    .AddLocalized("it", "IT")
                    .AddLocalized("es", "IT")
                    .AddLocalized("de", "EN");

            var result = FieldConverters.ResolveFallbackLanguages(config)(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_not_return_value_if_master_is_missing()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source =
                new ContentFieldData()
                    .AddLocalized("de", "DE");

            var expected =
                new ContentFieldData()
                    .AddLocalized("de", "DE");

            var result = FieldConverters.ResolveFallbackLanguages(languagesConfig)(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_filter_languages()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source =
                new ContentFieldData()
                    .AddLocalized("en", "EN")
                    .AddLocalized("de", "DE");

            var expected =
                new ContentFieldData()
                    .AddLocalized("de", "DE");

            var result = FieldConverters.FilterLanguages(languagesConfig, new[] { Language.DE })(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_master_language_if_languages_to_filter_are_invalid()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source =
                new ContentFieldData()
                    .AddLocalized("en", "EN")
                    .AddLocalized("de", "DE");

            var expected =
                new ContentFieldData()
                    .AddLocalized("en", "EN");

            var result = FieldConverters.FilterLanguages(languagesConfig, new[] { Language.CA })(source, field);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_return_same_values_if_resolving_fallback_languages_from_invariant_field()
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source = new ContentFieldData();

            var result = FieldConverters.ResolveFallbackLanguages(languagesConfig)(source, field);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_values_if_filtered_languages_is_empty()
        {
            var field = Fields.String(1, "string", Partitioning.Language);

            var source = new ContentFieldData();

            var result = FieldConverters.FilterLanguages(languagesConfig, Enumerable.Empty<Language>())(source, field);

            Assert.Same(source, result);
        }

        [Fact]
        public void Should_return_same_values_if_filtering_languages_from_invariant_field()
        {
            var field = Fields.String(1, "string", Partitioning.Invariant);

            var source = new ContentFieldData();

            var result = FieldConverters.FilterLanguages(languagesConfig, null)(source, field);

            Assert.Same(source, result);
        }
    }
}
