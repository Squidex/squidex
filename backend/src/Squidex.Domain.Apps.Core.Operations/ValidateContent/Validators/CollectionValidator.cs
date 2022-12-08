// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class CollectionValidator : IValidator
{
    private readonly bool isRequired;
    private readonly int? minItems;
    private readonly int? maxItems;

    public CollectionValidator(bool isRequired, int? minItems = null, int? maxItems = null)
    {
        if (minItems > maxItems)
        {
            ThrowHelper.ArgumentException("Min length must be greater than max length.", nameof(minItems));
        }

        this.isRequired = isRequired;
        this.minItems = minItems;
        this.maxItems = maxItems;
    }

    public void Validate(object? value, ValidationContext context)
    {
        if (value is not ICollection items || items.Count == 0)
        {
            if (isRequired && !context.IsOptional)
            {
                context.AddError(context.Path, T.Get("contents.validation.required"));
            }

            return;
        }

        if (minItems != null && maxItems != null)
        {
            if (minItems == maxItems && minItems != items.Count)
            {
                context.AddError(context.Path, T.Get("contents.validation.itemCount", new { count = minItems }));
            }
            else if (items.Count < minItems || items.Count > maxItems)
            {
                context.AddError(context.Path, T.Get("contents.validation.itemCountBetween", new { min = minItems, max = maxItems }));
            }
        }
        else
        {
            if (minItems != null && items.Count < minItems)
            {
                context.AddError(context.Path, T.Get("contents.validation.minItems", new { min = minItems }));
            }

            if (maxItems != null && items.Count > maxItems)
            {
                context.AddError(context.Path, T.Get("contents.validation.maxItems", new { max = maxItems }));
            }
        }
    }
}
