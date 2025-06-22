// ==========================================================================
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
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent;

public delegate ValidationContext ValidationUpdater(ValidationContext context);

public static class ValidationTestExtensions
{
    private static readonly NamedId<DomainId> AppId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private static readonly NamedId<DomainId> SchemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

    public static async ValueTask ValidateAsync(this IValidator validator,
        object? value,
        IList<string> errors,
        Schema? schema = null,
        ValidationMode mode = ValidationMode.Default,
        ValidationUpdater? updater = null,
        ValidationAction action = ValidationAction.Upsert,
        ResolvedComponents? components = null,
        DomainId? contentId = null,
        ContentData? previousData = null)
    {
        var context = CreateContext(schema, mode, updater, action, components, contentId, previousData);

        validator.Validate(value, context);

        await context.Root.CompleteAsync();

        AddErrors(context.Root, errors);
    }

    public static async ValueTask ValidateAsync(this IField field,
        object? value,
        IList<string> errors,
        Schema? schema = null,
        ValidationMode mode = ValidationMode.Default,
        ValidationUpdater? updater = null,
        IValidatorsFactory? factory = null,
        ValidationAction action = ValidationAction.Upsert,
        ResolvedComponents? components = null,
        DomainId? contentId = null,
        ContentData? previousData = null)
    {
        var context = CreateContext(schema, mode, updater, action, components, contentId, previousData);

        new ValidatorBuilder(factory, context).ValueValidator(field)
            .Validate(value, context);

        await context.Root.CompleteAsync();

        AddErrors(context.Root, errors);
    }

    public static async Task ValidatePartialAsync(this ContentData data,
        PartitionResolver partitionResolver,
        IList<ValidationError> errors,
        Schema? schema = null,
        ValidationMode mode = ValidationMode.Default,
        ValidationUpdater? updater = null,
        IValidatorsFactory? factory = null,
        ValidationAction action = ValidationAction.Upsert,
        ResolvedComponents? components = null,
        DomainId? contentId = null,
        ContentData? previousData = null)
    {
        var context = CreateContext(schema, mode, updater, action, components, contentId, previousData);

        await new ValidatorBuilder(factory, context).ContentValidator(partitionResolver)
            .ValidateInputPartialAsync(data);

        errors.AddRange(context.Root.Errors);
    }

    public static async Task ValidateAsync(this ContentData data,
        PartitionResolver partitionResolver,
        IList<ValidationError> errors,
        Schema? schema = null,
        ValidationMode mode = ValidationMode.Default,
        ValidationUpdater? updater = null,
        IValidatorsFactory? factory = null,
        ValidationAction action = ValidationAction.Upsert,
        ResolvedComponents? components = null,
        DomainId? contentId = null,
        ContentData? previousData = null)
    {
        var context = CreateContext(schema, mode, updater, action, components, contentId, previousData);

        await new ValidatorBuilder(factory, context).ContentValidator(partitionResolver)
            .ValidateInputAsync(data);

        errors.AddRange(context.Root.Errors);
    }

    private static void AddErrors(RootContext context, IList<string> errors)
    {
        foreach (var error in context.Errors)
        {
            var propertyName = error.PropertyNames?.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                errors.Add(error.Message);
            }
            else
            {
                errors.Add($"{propertyName}: {error.Message}");
            }
        }
    }

    private static ValidationContext CreateContext(
        Schema? schema,
        ValidationMode mode,
        ValidationUpdater? updater,
        ValidationAction action,
        ResolvedComponents? components,
        DomainId? contentId,
        ContentData? previousData)
    {
        schema ??= new Schema();

        var rootContext = new RootContext(
            new App { Id = AppId.Id, Name = AppId.Name },
            schema with { Id = SchemaId.Id, Name = SchemaId.Name },
            contentId ?? DomainId.NewGuid(),
            components ?? ResolvedComponents.Empty,
            TestUtils.DefaultSerializer)
        {
            PreviousData = previousData
        };

        var context =
            new ValidationContext(rootContext)
                .WithMode(mode)
                .WithAction(action);

        if (updater != null)
        {
            context = updater(context);
        }

        return context;
    }

    private sealed class ValidatorBuilder(IValidatorsFactory? validatorFactory, ValidationContext validationContext)
    {
        private static readonly IValidatorsFactory Default = new DefaultValidatorsFactory();

        public ContentValidator ContentValidator(PartitionResolver partitionResolver)
        {
            return new ContentValidator(partitionResolver, validationContext, CreateFactories());
        }

        private IValidator CreateValueValidator(IField field)
        {
            return new FieldValidator(new AggregateValidator(CreateValueValidators(field)), field);
        }

        public IValidator ValueValidator(IField field)
        {
            return CreateValueValidator(field);
        }

        private IEnumerable<IValidator> CreateValueValidators(IField field)
        {
            return CreateFactories().SelectMany(x => x.CreateValueValidators(validationContext, field, CreateValueValidator));
        }

        private IEnumerable<IValidatorsFactory> CreateFactories()
        {
            yield return Default;

            if (validatorFactory != null)
            {
                yield return validatorFactory;
            }
        }
    }
}
