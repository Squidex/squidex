// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class CollectionItemValidator : IValidator
{
    private readonly IValidator itemValidator;

    public CollectionItemValidator(IValidator itemValidator)
    {
        Guard.NotNull(itemValidator);

        this.itemValidator = itemValidator;
    }

    public void Validate(object? value, ValidationContext context)
    {
        if (value is ICollection { Count: > 0 } items)
        {
            var itemIndex = 1;

            foreach (var item in items)
            {
                var itemContext = context.Nested($"[{itemIndex}]");

                itemValidator.Validate(item, itemContext);
                itemIndex++;
            }
        }
    }
}
