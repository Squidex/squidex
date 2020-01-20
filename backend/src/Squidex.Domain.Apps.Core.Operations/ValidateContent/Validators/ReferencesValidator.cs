// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class ReferencesValidator : IValidator
    {
        private readonly IEnumerable<Guid>? schemaIds;

        public ReferencesValidator(IEnumerable<Guid>? schemaIds)
        {
            this.schemaIds = schemaIds;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (context.Mode == ValidationMode.Optimized)
            {
                return;
            }

            if (value is ICollection<Guid> contentIds)
            {
                var foundIds = await context.GetContentIdsAsync(contentIds.ToHashSet());

                foreach (var id in contentIds)
                {
                    var (schemaId, _) = foundIds.FirstOrDefault(x => x.Id == id);

                    if (schemaId == Guid.Empty)
                    {
                        addError(context.Path, $"Contains invalid reference '{id}'.");
                    }
                    else if (schemaIds?.Any() == true && !schemaIds.Contains(schemaId))
                    {
                        addError(context.Path, $"Contains reference '{id}' to invalid schema.");
                    }
                }
            }
        }
    }
}
