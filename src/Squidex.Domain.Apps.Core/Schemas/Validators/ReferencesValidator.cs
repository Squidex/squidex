// ==========================================================================
//  ReferencesValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class ReferencesValidator : IValidator
    {
        private readonly Guid schemaId;

        public ReferencesValidator(Guid schemaId)
        {
            this.schemaId = schemaId;
        }

        public async Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value is ICollection<Guid> contentIds)
            {
                var invalidIds = await context.GetInvalidContentIdsAsync(contentIds, schemaId);

                foreach (var invalidId in invalidIds)
                {
                    addError($"<FIELD> contains invalid reference '{invalidId}'.");
                }
            }
        }
    }
}
