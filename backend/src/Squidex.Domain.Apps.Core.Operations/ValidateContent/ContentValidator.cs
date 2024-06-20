// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed class ContentValidator
{
    private readonly PartitionResolver partitionResolver;
    private readonly ValidationContext context;
    private readonly IEnumerable<IValidatorsFactory> factories;

    public IEnumerable<ValidationError> Errors
    {
        get => context.Root.Errors;
    }

    public ContentValidator(PartitionResolver partitionResolver, ValidationContext context,
        IEnumerable<IValidatorsFactory> factories)
    {
        Guard.NotNull(context);
        Guard.NotNull(factories);
        Guard.NotNull(partitionResolver);

        this.context = context;
        this.factories = factories;
        this.partitionResolver = partitionResolver;
    }

    public ValueTask ValidateInputPartialAsync(ContentData data,
        CancellationToken ct = default)
    {
        Guard.NotNull(data);

        ValidateInputCore(data, true);

        return context.Root.CompleteAsync(ct);
    }

    public ValueTask ValidateInputAsync(ContentData data,
        CancellationToken ct = default)
    {
        Guard.NotNull(data);

        ValidateInputCore(data, false);

        return context.Root.CompleteAsync(ct);
    }

    public ValueTask ValidateInputAndContentAsync(ContentData data,
        CancellationToken ct = default)
    {
        Guard.NotNull(data);

        ValidateInputCore(data, false);
        ValidateContentCore(data);

        return context.Root.CompleteAsync(ct);
    }

    public ValueTask ValidateContentAsync(ContentData data,
        CancellationToken ct = default)
    {
        Guard.NotNull(data);

        ValidateContentCore(data);

        return context.Root.CompleteAsync(ct);
    }

    private void ValidateInputCore(ContentData data, bool partial)
    {
        CreateSchemaValidator(partial).Validate(data, context);
    }

    private void ValidateContentCore(ContentData data)
    {
        CreateSchemaValidator().Validate(data, context);
    }

    private IValidator CreateSchemaValidator()
    {
        return new AggregateValidator(CreateContentValidators());
    }

    private IValidator CreateSchemaValidator(bool isPartial)
    {
        var fieldValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(context.Root.Schema.Fields.Count);

        foreach (var field in context.Root.Schema.Fields)
        {
            fieldValidators[field.Name] = (!field.RawProperties.IsRequired, CreateFieldValidator(field, isPartial));
        }

        return new ObjectValidator<ContentFieldData>(fieldValidators, isPartial, "field");
    }

    private IValidator CreateFieldValidator(IRootField field, bool isPartial)
    {
        var valueValidator = CreateValueValidator(field);

        var partitioning = partitionResolver(field.Partitioning);
        var partitions = new Dictionary<string, (bool IsOptional, IValidator Validator)>();

        foreach (var partitionKey in partitioning.AllKeys)
        {
            var optional = partitioning.IsOptional(partitionKey);

            partitions[partitionKey] = (optional, valueValidator);
        }

        var typeName = partitioning.ToString()!;

        return new AggregateValidator(
            CreateFieldValidators(field)
                .Union(Enumerable.Repeat(
                    new ObjectValidator<JsonValue>(partitions, isPartial, typeName), 1)));
    }

    private IValidator CreateValueValidator(IField field)
    {
        return new FieldValidator(new AggregateValidator(CreateValueValidators(field)), field);
    }

    private IEnumerable<IValidator> CreateContentValidators()
    {
        return factories.SelectMany(x => x.CreateContentValidators(context, CreateValueValidator));
    }

    private IEnumerable<IValidator> CreateValueValidators(IField field)
    {
        return factories.SelectMany(x => x.CreateValueValidators(context, field, CreateValueValidator));
    }

    private IEnumerable<IValidator> CreateFieldValidators(IField field)
    {
        return factories.SelectMany(x => x.CreateFieldValidators(context, field, CreateValueValidator));
    }
}
