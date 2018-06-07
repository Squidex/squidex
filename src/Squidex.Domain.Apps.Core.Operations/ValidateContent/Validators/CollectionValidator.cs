// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class CollectionValidator : IValidator
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

        public Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (!(value is ICollection items) || items.Count == 0)
            {
                if (isRequired && !context.IsOptional)
                {
                    addError(context.Path, "Field is required.");
                }

                return TaskHelper.Done;
            }

            if (minItems.HasValue && items.Count < minItems.Value)
            {
                addError(context.Path, $"Must have at least {minItems} item(s).");
            }

            if (maxItems.HasValue && items.Count > maxItems.Value)
            {
                addError(context.Path, $"Must have not more than {maxItems} item(s).");
            }

            return TaskHelper.Done;
        }
    }
}
