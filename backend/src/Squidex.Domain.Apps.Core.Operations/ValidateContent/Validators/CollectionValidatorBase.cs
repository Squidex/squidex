// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public abstract class CollectionValidatorBase
    {
        private readonly bool isRequired;
        private readonly int? minItems;
        private readonly int? maxItems;

        protected CollectionValidatorBase(bool isRequired, int? minItems = null, int? maxItems = null)
        {
            if (minItems.HasValue && maxItems.HasValue && minItems > maxItems)
            {
                throw new ArgumentException("Min length must be greater than max length.", nameof(minItems));
            }

            this.isRequired = isRequired;
            this.minItems = minItems;
            this.maxItems = maxItems;
        }

        protected void ValidateRequired(object? value, ValidationContext context, AddError addError)
        {
            var size = 0;

            if (value is ICollection items)
            {
                size = items.Count;
            }

            if (size == 0 && isRequired && !context.IsOptional)
            {
                addError(context.Path, T.Get("contents.validation.required"));
            }
        }

        protected void ValidateSize(object? value, ValidationContext context, AddError addError)
        {
            var size = 0;

            if (value is ICollection items)
            {
                size = items.Count;
            }

            if (size == 0)
            {
                return;
            }

            if (minItems.HasValue && maxItems.HasValue)
            {
                if (minItems == maxItems && minItems != size)
                {
                    addError(context.Path, T.Get("contents.validation.itemCount", new { count = minItems }));
                }
                else if (size < minItems || size > maxItems)
                {
                    addError(context.Path, T.Get("contents.validation.itemCountBetween", new { min = minItems, max = maxItems }));
                }
            }
            else
            {
                if (minItems.HasValue && size < minItems)
                {
                    addError(context.Path, T.Get("contents.validation.minItems", new { min = minItems }));
                }

                if (maxItems.HasValue && size > maxItems)
                {
                    addError(context.Path, T.Get("contents.validation.maxItems", new { max = maxItems }));
                }
            }
        }
    }
}