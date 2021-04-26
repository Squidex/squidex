// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public delegate Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> CheckContentsByIds(HashSet<DomainId> ids);

    public sealed class ReferencesValidator : IValidator
    {
        private readonly ReferencesFieldProperties properties;
        private readonly CollectionValidator? collectionValidator;
        private readonly UniqueValuesValidator<DomainId>? uniqueValidator;
        private readonly CheckContentsByIds checkReferences;

        public ReferencesValidator(bool isRequired, ReferencesFieldProperties properties, CheckContentsByIds checkReferences)
        {
            Guard.NotNull(properties, nameof(properties));
            Guard.NotNull(checkReferences, nameof(checkReferences));

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

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            var foundIds = new List<DomainId>();

            if (value is ICollection<DomainId> { Count: > 0 } contentIds)
            {
                var references = await checkReferences(contentIds.ToHashSet());
                var index = 0;

                foreach (var id in contentIds)
                {
                    index++;

                    var path = context.Path.Enqueue($"[{index}]");

                    var (schemaId, _, status) = references.FirstOrDefault(x => x.Id == id);

                    if (schemaId == DomainId.Empty)
                    {
                        if (context.Action == ValidationAction.Upsert)
                        {
                            addError(path, T.Get("contents.validation.referenceNotFound", new { id }));
                        }

                        continue;
                    }

                    var isValid = true;

                    if (properties.SchemaIds?.Any() == true && !properties.SchemaIds.Contains(schemaId))
                    {
                        if (context.Action == ValidationAction.Upsert)
                        {
                            addError(path, T.Get("contents.validation.referenceToInvalidSchema", new { id }));
                        }

                        isValid = false;
                    }

                    isValid &= !properties.MustBePublished || status == Status.Published;

                    if (isValid)
                    {
                        foundIds.Add(id);
                    }
                }
            }

            if (collectionValidator != null)
            {
                await collectionValidator.ValidateAsync(foundIds, context, addError);
            }

            if (uniqueValidator != null)
            {
                await uniqueValidator.ValidateAsync(foundIds, context, addError);
            }
        }
    }
}
