// ==========================================================================
//  ReferencesValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class ReferencesValidator : IValidator
    {
        private readonly bool isRequired;
        private readonly Guid schemaId;
        private readonly int? minItems;
        private readonly int? maxItems;

        public ReferencesValidator(bool isRequired, Guid schemaId, int? minItems = null, int? maxItems = null)
        {
            this.isRequired = isRequired;
            this.schemaId = schemaId;
            this.minItems = minItems;
            this.maxItems = maxItems;
        }

        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            var references = value as ReferencesValue;

            if (references == null || references.ContentIds.Count == 0)
            {
                if (isRequired && !context.IsOptional)
                {
                    addError("<FIELD> is required");
                }

                return;
            }

            if (minItems.HasValue && references.ContentIds.Count < minItems.Value)
            {
                addError($"<FIELD> must have at least {minItems} reference(s)");
            }

            if (maxItems.HasValue && references.ContentIds.Count > maxItems.Value)
            {
                addError($"<FIELD> must have not more than {maxItems} reference(s)");
            }

            var invalidIds = await context.GetInvalidContentIdsAsync(references.ContentIds, schemaId);

            foreach (var invalidId in invalidIds)
            {
                addError($"<FIELD> contains invalid reference '{invalidId}'");
            }
        }
    }
}
