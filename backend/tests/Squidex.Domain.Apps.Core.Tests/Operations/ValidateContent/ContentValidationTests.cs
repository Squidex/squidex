﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public class ContentValidationTests : IClassFixture<TranslationsFixture>
{
    private readonly LanguagesConfig languages = LanguagesConfig.English.Set(Language.DE);
    private readonly List<ValidationError> errors = [];
    private Schema schema = new Schema { Name = "my-schema" };

    [Fact]
    public async Task Should_add_error_if_value_validator_throws_exception()
    {
        var validator = A.Fake<IValidator>();

        A.CallTo(() => validator.Validate(A<object?>._, A<ValidationContext>._))
            .Throws(new InvalidOperationException());

        var validatorFactory = A.Fake<IValidatorsFactory>();

        A.CallTo(() => validatorFactory.CreateValueValidators(A<ValidationContext>._, A<IField>._, A<ValidatorFactory>._))
            .Returns(Enumerable.Repeat(validator, 1));

        schema = schema.AddNumber(1, "myField", Partitioning.Invariant,
            new NumberFieldProperties());

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant(1000));

        await data.ValidateAsync(languages.ToResolver(), errors, schema, factory: validatorFactory);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Validation failed with internal error.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_field_validator_throws_exception()
    {
        var validator = A.Fake<IValidator>();

        A.CallTo(() => validator.Validate(A<object?>._, A<ValidationContext>._))
            .Throws(new InvalidOperationException());

        var validatorFactory = A.Fake<IValidatorsFactory>();

        A.CallTo(() => validatorFactory.CreateFieldValidators(A<ValidationContext>._, A<IField>._, A<ValidatorFactory>._))
            .Returns(Enumerable.Repeat(validator, 1));

        schema = schema.AddNumber(1, "myField", Partitioning.Invariant,
            new NumberFieldProperties());

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant(1000));

        await data.ValidateAsync(languages.ToResolver(), errors, schema, factory: validatorFactory);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Validation failed with internal error.", "myField"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_validating_data_with_unknown_field()
    {
        var data =
            new ContentData()
                .AddField("unknown",
                    []);

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known field.", "unknown"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_validating_data_with_invalid_field()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Invariant,
            new NumberFieldProperties { MaxValue = 100 });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant(1000));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Must be less or equal to 100.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_non_localizable_data_field_contains_language()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("es", 1)
                        .AddLocalized("it", 1));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known invariant value.", "myField.es"),
                new ValidationError("Not a known invariant value.", "myField.it"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_validating_data_with_invalid_localizable_field()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Language,
            new NumberFieldProperties { IsRequired = true });

        var data =
            new ContentData();

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.de"),
                new ValidationError("Field is required.", "myField.en"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_required_data_field_is_not_in_bag()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Invariant,
            new NumberFieldProperties { IsRequired = true });

        var data =
            new ContentData();

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_required_data_string_field_is_not_in_bag()
    {
        schema = schema.AddString(1, "myField", Partitioning.Invariant,
            new StringFieldProperties { IsRequired = true });

        var data =
            new ContentData();

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_data_contains_invalid_language()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Language);

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("de", 1)
                        .AddLocalized("ru", 1));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known language.", "myField.ru"),
            ]);
    }

    [Fact]
    public async Task Should_not_add_error_if_required_field_has_no_value_for_optional_language()
    {
        var optionalConfig =
            LanguagesConfig.English
                .Set(Language.ES)
                .Set(Language.IT, true)
                .Remove(Language.EN);

        schema = schema.AddString(1, "myField", Partitioning.Language,
            new StringFieldProperties { IsRequired = true });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("es", "value"));

        await data.ValidateAsync(optionalConfig.ToResolver(), errors, schema);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_data_contains_unsupported_language()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Language);

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("es", 1)
                        .AddLocalized("it", 1));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known language.", "myField.es"),
                new ValidationError("Not a known language.", "myField.it"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_validating_partial_data_with_unknown_field()
    {
        var data =
            new ContentData()
                .AddField("unknown",
                    []);

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known field.", "unknown"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_validating_partial_data_with_invalid_field()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Invariant,
            new NumberFieldProperties { MaxValue = 100 });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant(1000));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Must be less or equal to 100.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_non_localizable_partial_data_field_contains_language()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Invariant);

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("es", 1)
                        .AddLocalized("it", 1));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known invariant value.", "myField.es"),
                new ValidationError("Not a known invariant value.", "myField.it"),
            ]);
    }

    [Fact]
    public async Task Should_not_add_error_if_validating_partial_data_with_invalid_localizable_field()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Language,
            new NumberFieldProperties { IsRequired = true });

        var data =
            new ContentData();

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_required_partial_data_field_is_not_in_bag()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Invariant,
            new NumberFieldProperties { IsRequired = true });

        var data =
            new ContentData();

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_partial_data_contains_invalid_language()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Language);

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("de", 1)
                        .AddLocalized("ru", 1));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known language.", "myField.ru"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_partial_data_contains_unsupported_language()
    {
        schema = schema.AddNumber(1, "myField", Partitioning.Language);

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("es", 1)
                        .AddLocalized("it", 1));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Not a known language.", "myField.es"),
                new ValidationError("Not a known language.", "myField.it"),
            ]);
    }

    [Fact]
    public async Task Should_add_error_if_array_field_has_required_nested_field()
    {
        schema = schema.AddArray(1, "myField", Partitioning.Invariant, f => f.
            AddNumber(2, "myNested", new NumberFieldProperties { IsRequired = true }));

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object(),
                                JsonValue.Object().Add("myNested", 1),
                                JsonValue.Object())));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv[1].myNested"),
                new ValidationError("Field is required.", "myField.iv[3].myNested"),
            ]);
    }

    [Fact]
    public async Task Should_not_add_error_if_separator_not_defined()
    {
        schema = schema.AddUI(2, "ui", Partitioning.Invariant);

        var data =
            new ContentData();

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_nested_separator_not_defined()
    {
        schema = schema.AddArray(1, "myField", Partitioning.Invariant, f => f.
            AddUI(2, "myNested"));

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object())));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_validate_partial_unset_field_as_null()
    {
        schema = schema.AddString(1, "myField", Partitioning.Invariant,
            new StringFieldProperties { IsRequired = true });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("$unset", true));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_handle_partial_unset_field_value_as_null()
    {
        schema = schema.AddString(1, "myField", Partitioning.Invariant,
            new StringFieldProperties { IsRequired = true });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("iv",
                            JsonValue.Object()
                                .Add("$unset", true)));

        await data.ValidatePartialAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_validate_unset_field_as_null()
    {
        schema = schema.AddString(1, "myField", Partitioning.Invariant,
            new StringFieldProperties { IsRequired = true });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("$unset", true));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_handle_unset_field_value_as_null()
    {
        schema = schema.AddString(1, "myField", Partitioning.Invariant,
            new StringFieldProperties { IsRequired = true });

        var data =
            new ContentData()
                .AddField("myField",
                    new ContentFieldData()
                        .AddLocalized("iv",
                            JsonValue.Object()
                                .Add("$unset", true)));

        await data.ValidateAsync(languages.ToResolver(), errors, schema);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field is required.", "myField.iv"),
            ]);
    }

    [Fact]
    public async Task Should_not_allow_changed_fields()
    {
        schema = schema.AddString(1, "myField1", Partitioning.Invariant,
            new StringFieldProperties { IsCreateOnly = true });

        schema = schema.AddString(2, "myField2", Partitioning.Invariant,
            new StringFieldProperties { IsCreateOnly = true });

        schema = schema.AddString(3, "myField3", Partitioning.Invariant,
            new StringFieldProperties());

        var data =
            new ContentData()
                .AddField("myField1",
                    new ContentFieldData()
                        .AddInvariant("Value1_0"))
                .AddField("myField2",
                    new ContentFieldData()
                        .AddInvariant("Value2_0"))
                .AddField("myField3",
                    new ContentFieldData()
                        .AddInvariant("Value3_0"));

        var previousData =
            new ContentData()
                .AddField("myField1",
                    new ContentFieldData()
                        .AddInvariant("Value1_1"))
                .AddField("myField2",
                    new ContentFieldData()
                        .AddInvariant("Value2_0"))
                .AddField("myField3",
                    new ContentFieldData()
                        .AddInvariant("Value3_0"));

        await data.ValidateAsync(languages.ToResolver(), errors, schema, previousData: previousData);

        errors.Should().BeEquivalentTo(
            [
                new ValidationError("Field cannot be changed after creation.", "myField1.iv"),
            ]);
    }
}
