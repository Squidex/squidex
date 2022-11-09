// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public delegate Task<IReadOnlyList<ContentIdStatus>> CheckContentsByIds(HashSet<DomainId> ids);

public sealed class ReferencesValidator : IValidator
{
    private readonly ReferencesFieldProperties properties;
    private readonly CollectionValidator? collectionValidator;
    private readonly UniqueValuesValidator<DomainId>? uniqueValidator;
    private readonly CheckContentsByIds checkReferences;

    public ReferencesValidator(bool isRequired, ReferencesFieldProperties properties, CheckContentsByIds checkReferences)
    {
        Guard.NotNull(properties);
        Guard.NotNull(checkReferences);

        this.properties = properties;

        if (isRequired || properties.MinItems != null || properties.MaxItems != null)
        {
            collectionValidator = new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
        }

        if (!properties.AllowDuplicates)
        {
            uniqueValidator = new UniqueValuesValidator<DomainId>();
        }

        this.checkReferences = checkReferences;
    }

    public void Validate(object? value, ValidationContext context)
    {
        context.Root.AddTask(ct => ValidateCoreAsync(value, context));
    }

    private async Task ValidateCoreAsync(object? value, ValidationContext context)
    {
        var foundIds = new List<DomainId>();

        if (value is ICollection<DomainId> { Count: > 0 } contentIds)
        {
            var references = await checkReferences(contentIds.ToHashSet());
            var referenceIndex = 1;

            foreach (var id in contentIds)
            {
                var path = context.Path.Enqueue($"[{referenceIndex}]");

                var (schemaId, _, status) = references.FirstOrDefault(x => x.Id == id);

                if (schemaId == DomainId.Empty)
                {
                    if (context.Action == ValidationAction.Upsert)
                    {
                        context.AddError(path, T.Get("contents.validation.referenceNotFound", new { id }));
                    }

                    continue;
                }

                var isValid = true;

                if (properties.SchemaIds?.Any() == true && !properties.SchemaIds.Contains(schemaId))
                {
                    if (context.Action == ValidationAction.Upsert)
                    {
                        context.AddError(path, T.Get("contents.validation.referenceToInvalidSchema", new { id }));
                    }

                    isValid = false;
                }

                isValid &= !properties.MustBePublished || status == Status.Published;

                if (isValid)
                {
                    foundIds.Add(id);
                }

                referenceIndex++;
            }
        }

        if (collectionValidator != null)
        {
            collectionValidator.Validate(foundIds, context);
        }

        if (uniqueValidator != null)
        {
            uniqueValidator.Validate(foundIds, context);
        }
    }
}
