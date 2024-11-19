// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class ComponentValidator(Func<Schema, IValidator?> validatorFactory) : IValidator
{
    public void Validate(object? value, ValidationContext context)
    {
        if (value is Component component)
        {
            var validator = validatorFactory(component.Schema);

            validator?.Validate(component.Data, context);
        }
    }
}
