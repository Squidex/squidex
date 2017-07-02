// ==========================================================================
//  ReferencesValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public sealed class ReferencesValidator : IValidator
    {
        private readonly bool isRequired;
        private readonly Guid schemaId;

        public ReferencesValidator(bool isRequired, Guid schemaId)
        {
            this.isRequired = isRequired;
            this.schemaId = schemaId;
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

            var invalidIds = await context.GetInvalidContentIdsAsync(references.ContentIds, schemaId);
            
            foreach (var invalidId in invalidIds)
            {
                addError($"<FIELD> contains invalid reference '{invalidId}'");
            }
        }
    }
}
