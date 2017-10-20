// ==========================================================================
//  CollectionValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class CollectionValidator<T> : IValidator
    {
        private readonly bool isRequired;
        private readonly int? minItems;
        private readonly int? maxItems;

        public CollectionValidator(bool isRequired, int? minItems = null, int? maxItems = null)
        {
            this.isRequired = isRequired;
            this.minItems = minItems;
            this.maxItems = maxItems;
        }

        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (!(value is ICollection<T> items) || items.Count == 0)
            {
                if (isRequired && !context.IsOptional)
                {
                    addError("<FIELD> is required.");
                }

                return TaskHelper.Done;
            }

            if (minItems.HasValue && items.Count < minItems.Value)
            {
                addError($"<FIELD> must have at least {minItems} item(s).");
            }

            if (maxItems.HasValue && items.Count > maxItems.Value)
            {
                addError($"<FIELD> must have not more than {maxItems} item(s).");
            }

            return TaskHelper.Done;
        }
    }
}
