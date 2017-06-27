// ==========================================================================
//  ReferencesValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
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

            var referenceTasks = references.ContentIds.Select(x => CheckReferenceAsync(context, x)).ToArray();

            await Task.WhenAll(referenceTasks);

            foreach (var notFoundId in referenceTasks.Where(x => !x.Result.IsFound).Select(x => x.Result.ReferenceId))
            {
                addError($"<FIELD> contains invalid reference '{notFoundId}'");
            }
        }

        private async Task<(Guid ReferenceId, bool IsFound)> CheckReferenceAsync(ValidationContext context, Guid id)
        {
            var isFound = await context.IsValidContentIdAsync(schemaId, id);

            return (id, isFound);
        }
    }
}
