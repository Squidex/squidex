// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
            if (minItems.HasValue && maxItems.HasValue && minItems.Value > maxItems.Value)
            {
                throw new ArgumentException("Min length must be greater than max length.", nameof(minItems));
            }

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

            if (minItems.HasValue && maxItems.HasValue)
            {
                if (minItems == maxItems && minItems != items.Count)
                {
                    addError(context.Path, $"Must have exactly {maxItems} item(s).");
                }
                else if (items.Count < minItems || items.Count > maxItems)
                {
                    addError(context.Path, $"Must have between {minItems} and {maxItems} item(s).");
                }
            }
            else
            {
                if (minItems.HasValue && items.Count < minItems.Value)
                {
                    addError(context.Path, $"Must have at least {minItems} item(s).");
                }

                if (maxItems.HasValue && items.Count > maxItems.Value)
                {
                    addError(context.Path, $"Must not have more than {maxItems} item(s).");
                }
            }

            return TaskHelper.Done;
        }
    }
}
