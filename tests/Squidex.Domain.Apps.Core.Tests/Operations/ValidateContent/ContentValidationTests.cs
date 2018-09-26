// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ContentValidationTests
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.DE, Language.EN);
        private readonly List<ValidationError> errors = new List<ValidationError>();
        private readonly ValidationContext context = ValidationTestExtensions.ValidContext;
        private Schema schema = new Schema("my-schema");

        [Fact]
        public async Task Should_add_error_if_validating_data_with_unknown_field()
        {
            var data =
                new NamedContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown: Not a known field.", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_field()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties { MaxValue = 100 });

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue(1000));

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field: Must be less than or equal to '100'.", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_non_localizable_data_field_contains_language()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant);

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(es): Not a known invariant value.", "my-field(es)"),
                    new ValidationError("my-field(it): Not a known invariant value.", "my-field(it)")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_localizable_field()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(de): Field is required.", "my-field(de)"),
                    new ValidationError("my-field(en): Field is required.", "my-field(en)")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_data_field_is_not_in_bag()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field: Field is required.", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_data_contains_invalid_language()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language);

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("xx", 1));

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(xx): Not a known language.", "my-field(xx)")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_required_field_has_no_value_for_optional_language()
        {
            var optionalConfig =
                LanguagesConfig.Build(
                    new LanguageConfig(Language.ES, false),
                    new LanguageConfig(Language.IT, true));

            schema = schema.AddString(1, "my-field", Partitioning.Language,
                new StringFieldProperties { IsRequired = true });

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", "value"));

            await data.ValidateAsync(context, schema, optionalConfig.ToResolver(), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_data_contains_unsupported_language()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language);

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidateAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(es): Not a known language.", "my-field(es)"),
                    new ValidationError("my-field(it): Not a known language.", "my-field(it)")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_unknown_field()
        {
            var data =
                new NamedContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown: Not a known field.", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_invalid_field()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties { MaxValue = 100 });

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue(1000));

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field: Must be less than or equal to '100'.", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_non_localizable_partial_data_field_contains_language()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant);

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(es): Not a known invariant value.", "my-field(es)"),
                    new ValidationError("my-field(it): Not a known invariant value.", "my-field(it)")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_validating_partial_data_with_invalid_localizable_field()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_required_partial_data_field_is_not_in_bag()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_add_error_if_partial_data_contains_invalid_language()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language);

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("xx", 1));

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(xx): Not a known language.", "my-field(xx)")
                });
        }

        [Fact]
        public async Task Should_add_error_if_partial_data_contains_unsupported_language()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language);

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field(es): Not a known language.", "my-field(es)"),
                    new ValidationError("my-field(it): Not a known language.", "my-field(it)")
                });
        }

        [Fact]
        public async Task Should_add_error_if_array_field_has_required_nested_field()
        {
            schema =
                schema.AddArray(1, "my-field", Partitioning.Invariant, f => f.
                    AddNumber(1, "my-nested", new NumberFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("iv", new JArray(
                                new JObject(),
                                new JObject(new JProperty("my-nested", 1)),
                                new JObject())));

            await data.ValidatePartialAsync(context, schema, languagesConfig.ToResolver(), errors);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field[1].my-nested: Field is required.", "my-field[1].my-nested"),
                    new ValidationError("my-field[3].my-nested: Field is required.", "my-field[3].my-nested")
                });
        }
    }
}
