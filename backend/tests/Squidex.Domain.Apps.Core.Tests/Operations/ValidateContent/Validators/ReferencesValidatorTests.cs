// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent.Validators;

public class ReferencesValidatorTests : IClassFixture<TranslationsFixture>
{
    private readonly List<string> errors = new List<string>();
    private readonly DomainId schemaId1 = DomainId.NewGuid();
    private readonly DomainId schemaId2 = DomainId.NewGuid();
    private readonly DomainId ref1 = DomainId.NewGuid();
    private readonly DomainId ref2 = DomainId.NewGuid();

    [Fact]
    public async Task Should_not_add_error_if_reference_invalid_but_publishing()
    {
        var properties = new ReferencesFieldProperties { SchemaId = schemaId1 };

        var sut = Validator(properties, schemaId2, (ref2, Status.Published));

        await sut.ValidateAsync(CreateValue(ref2), errors, action: ValidationAction.Publish);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_schemas_not_defined()
    {
        var properties = new ReferencesFieldProperties();

        var sut = Validator(properties, schemaId2, (ref2, Status.Published));

        await sut.ValidateAsync(CreateValue(ref2), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_schema_is_valid()
    {
        var properties = new ReferencesFieldProperties { SchemaId = schemaId1 };

        var sut = Validator(properties, schemaId1, (ref2, Status.Published));

        await sut.ValidateAsync(CreateValue(ref2), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_references_are_null_but_not_required()
    {
        var properties = new ReferencesFieldProperties();

        var sut = Validator(properties);

        await sut.ValidateAsync(null, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_references_are_empty_but_not_required()
    {
        var properties = new ReferencesFieldProperties();

        var sut = Validator(properties);

        await sut.ValidateAsync(CreateValue(), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_add_error_if_duplicates_are_allowed()
    {
        var properties = new ReferencesFieldProperties { AllowDuplicates = true };

        var sut = Validator(properties, schemaId1, (ref1, Status.Published));

        await sut.ValidateAsync(CreateValue(ref1, ref1), errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_add_error_if_references_are_required()
    {
        var properties = new ReferencesFieldProperties { IsRequired = true };

        var sut = Validator(properties, schemaId1);

        await sut.ValidateAsync(CreateValue(), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_references_are_published_required()
    {
        var properties = new ReferencesFieldProperties { MustBePublished = true, IsRequired = true };

        var sut = Validator(properties, schemaId1, (ref1, Status.Published));

        await sut.ValidateAsync(CreateValue(), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Field is required." });
    }

    [Fact]
    public async Task Should_add_error_if_references_are_not_valid()
    {
        var properties = new ReferencesFieldProperties();

        var sut = Validator(properties);

        await sut.ValidateAsync(CreateValue(ref1), errors);

        errors.Should().BeEquivalentTo(
            new[] { $"[1]: Reference '{ref1}' not found." });
    }

    [Fact]
    public async Task Should_add_error_if_reference_schema_is_not_valid()
    {
        var properties = new ReferencesFieldProperties { SchemaId = schemaId1 };

        var sut = Validator(properties, schemaId2, (ref2, Status.Draft));

        await sut.ValidateAsync(CreateValue(ref2), errors);

        errors.Should().BeEquivalentTo(
            new[] { $"[1]: Reference '{ref2}' has invalid schema." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_not_enough_items()
    {
        var properties = new ReferencesFieldProperties { MinItems = 2 };

        var sut = Validator(properties, schemaId2, (ref2, Status.Draft));

        await sut.ValidateAsync(CreateValue(ref2), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 2 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_not_enough_published_items()
    {
        var properties = new ReferencesFieldProperties { MinItems = 2, MustBePublished = true };

        var sut = Validator(properties, schemaId1, (ref1, Status.Published), (ref2, Status.Draft));

        await sut.ValidateAsync(CreateValue(ref1, ref2), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must have at least 2 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_value_has_too_much_items()
    {
        var properties = new ReferencesFieldProperties { MaxItems = 1 };

        var sut = Validator(properties, schemaId1, (ref1, Status.Published), (ref2, Status.Draft));

        await sut.ValidateAsync(CreateValue(ref1, ref2), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not have more than 1 item(s)." });
    }

    [Fact]
    public async Task Should_add_error_if_reference_contains_duplicate_values()
    {
        var properties = new ReferencesFieldProperties();

        var sut = Validator(properties, schemaId1, (ref1, Status.Published));

        await sut.ValidateAsync(CreateValue(ref1, ref1), errors);

        errors.Should().BeEquivalentTo(
            new[] { "Must not contain duplicate values." });
    }

    private static List<DomainId> CreateValue(params DomainId[] ids)
    {
        return ids.ToList();
    }

    private IValidator Validator(ReferencesFieldProperties properties)
    {
        return new ReferencesValidator(properties.IsRequired, properties, FoundReferences(schemaId1));
    }

    private static IValidator Validator(ReferencesFieldProperties properties, DomainId schemaId, params (DomainId Id, Status Status)[] references)
    {
        return new ReferencesValidator(properties.IsRequired, properties, FoundReferences(schemaId, references));
    }

    private static CheckContentsByIds FoundReferences(DomainId schemaId, params (DomainId Id, Status Status)[] references)
    {
        return x =>
        {
            var actual = references.Select(x => new ContentIdStatus(schemaId, x.Id, x.Status)).ToList();

            return Task.FromResult<IReadOnlyList<ContentIdStatus>>(actual);
        };
    }
}
