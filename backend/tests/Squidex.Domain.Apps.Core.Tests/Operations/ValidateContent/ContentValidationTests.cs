﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public class ContentValidationTests : IClassFixture<TranslationsFixture>
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.English.Set(Language.DE);
        private readonly List<ValidationError> errors = new List<ValidationError>();
        private Schema schema = new Schema("my-schema");

        [Fact]
        public async Task Should_add_error_if_value_validator_throws_exception()
        {
            var validator = A.Fake<IValidator>();

            A.CallTo(() => validator.ValidateAsync(A<object?>._, A<ValidationContext>._, A<AddError>._))
                .Throws(new InvalidOperationException());

            var validatorFactory = A.Fake<IValidatorsFactory>();

            A.CallTo(() => validatorFactory.CreateValueValidators(A<ValidationContext>._, A<IField>._, A<ValidatorFactory>._))
                .Returns(Enumerable.Repeat(validator, 1));

            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties());

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("iv", 1000));

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema, factory: validatorFactory);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Validation failed with internal error.", "my-field.iv")
                });
        }

        [Fact]
        public async Task Should_add_error_if_field_validator_throws_exception()
        {
            var validator = A.Fake<IValidator>();

            A.CallTo(() => validator.ValidateAsync(A<object?>._, A<ValidationContext>._, A<AddError>._))
                .Throws(new InvalidOperationException());

            var validatorFactory = A.Fake<IValidatorsFactory>();

            A.CallTo(() => validatorFactory.CreateFieldValidators(A<ValidationContext>._, A<IField>._, A<ValidatorFactory>._))
                .Returns(Enumerable.Repeat(validator, 1));

            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties());

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("iv", 1000));

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema, factory: validatorFactory);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Validation failed with internal error.", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_unknown_field()
        {
            var data =
                new NamedContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known field.", "unknown")
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
                            .AddValue("iv", 1000));

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Must be less or equal to 100.", "my-field.iv")
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

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known invariant value.", "my-field.es"),
                    new ValidationError("Not a known invariant value.", "my-field.it")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_localizable_field()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Field is required.", "my-field.de"),
                    new ValidationError("Field is required.", "my-field.en")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_data_field_is_not_in_bag()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Field is required.", "my-field.iv")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_data_string_field_is_not_in_bag()
        {
            schema = schema.AddString(1, "my-field", Partitioning.Invariant,
                new StringFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Field is required.", "my-field.iv")
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
                            .AddValue("ru", 1));

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known language.", "my-field.ru")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_required_field_has_no_value_for_optional_language()
        {
            var optionalConfig =
                LanguagesConfig.English
                    .Set(Language.ES)
                    .Set(Language.IT, true)
                    .Remove(Language.EN);

            schema = schema.AddString(1, "my-field", Partitioning.Language,
                new StringFieldProperties { IsRequired = true });

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", "value"));

            await data.ValidateAsync(optionalConfig.ToResolver(), errors, schema);

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

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known language.", "my-field.es"),
                    new ValidationError("Not a known language.", "my-field.it")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_unknown_field()
        {
            var data =
                new NamedContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known field.", "unknown")
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
                            .AddValue("iv", 1000));

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Must be less or equal to 100.", "my-field.iv")
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

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known invariant value.", "my-field.es"),
                    new ValidationError("Not a known invariant value.", "my-field.it")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_validating_partial_data_with_invalid_localizable_field()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Language,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_required_partial_data_field_is_not_in_bag()
        {
            schema = schema.AddNumber(1, "my-field", Partitioning.Invariant,
                new NumberFieldProperties { IsRequired = true });

            var data =
                new NamedContentData();

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

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
                            .AddValue("ru", 1));

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known language.", "my-field.ru")
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

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Not a known language.", "my-field.es"),
                    new ValidationError("Not a known language.", "my-field.it")
                });
        }

        [Fact]
        public async Task Should_add_error_if_array_field_has_required_nested_field()
        {
            schema = schema.AddArray(1, "my-field", Partitioning.Invariant, f => f.
                AddNumber(2, "my-nested", new NumberFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddJsonValue(
                                JsonValue.Array(
                                    JsonValue.Object(),
                                    JsonValue.Object().Add("my-nested", 1),
                                    JsonValue.Object())));

            await data.ValidatePartialAsync(languagesConfig.ToResolver(), errors, schema);

            errors.Should().BeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("Field is required.", "my-field.iv[1].my-nested"),
                    new ValidationError("Field is required.", "my-field.iv[3].my-nested")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_separator_not_defined()
        {
            schema = schema.AddUI(2, "ui", Partitioning.Invariant);

            var data =
                new NamedContentData();

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_nested_separator_not_defined()
        {
            schema = schema.AddArray(1, "my-field", Partitioning.Invariant, f => f.
                AddUI(2, "my-nested"));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddJsonValue(
                                JsonValue.Array(
                                    JsonValue.Object())));

            await data.ValidateAsync(languagesConfig.ToResolver(), errors, schema);

            Assert.Empty(errors);
        }
    }
}
