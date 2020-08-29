// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public delegate Task<IReadOnlyList<(DomainId SchemaId, DomainId Id)>> CheckContentsByIds(HashSet<DomainId> ids);

    public sealed class ReferencesValidator : IValidator
    {
        private readonly IEnumerable<DomainId>? schemaIds;
        private readonly CheckContentsByIds checkReferences;

        public ReferencesValidator(IEnumerable<DomainId>? schemaIds, CheckContentsByIds checkReferences)
        {
            Guard.NotNull(checkReferences, nameof(checkReferences));

            this.schemaIds = schemaIds;

            this.checkReferences = checkReferences;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (context.Mode == ValidationMode.Optimized)
            {
                return;
            }

            if (value is ICollection<DomainId> contentIds)
            {
                var foundIds = await checkReferences(contentIds.ToHashSet());

                foreach (var id in contentIds)
                {
                    var (schemaId, _) = foundIds.FirstOrDefault(x => x.Id == id);

                    if (schemaId == DomainId.Empty)
                    {
                        addError(context.Path, T.Get("common.referenceNotFound", new { id }));
                    }
                    else if (schemaIds?.Any() == true && !schemaIds.Contains(schemaId))
                    {
                        addError(context.Path, T.Get("common.referenceToInvalidSchema", new { id }));
                    }
                }
            }
        }
    }
}
