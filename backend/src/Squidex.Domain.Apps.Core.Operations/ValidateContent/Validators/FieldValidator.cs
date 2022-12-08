// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class FieldValidator : IValidator
{
    private readonly IValidator validator;
    private readonly IField field;

    public FieldValidator(IValidator validator, IField field)
    {
        Guard.NotNull(field);
        Guard.NotNull(validator);

        this.field = field;
        this.validator = validator;
    }

    public void Validate(object? value, ValidationContext context)
    {
        var typedValue = value;

        try
        {
            if (value is JsonValue jsonValue)
            {
                if (jsonValue == default)
                {
                    typedValue = null;
                }
                else
                {
                    typedValue = jsonValue.Value;

                    var (json, error) = JsonValueConverter.ConvertValue(field, jsonValue,
                        context.Root.Serializer,
                        context.Root.Components);

                    if (error != null)
                    {
                        context.AddError(context.Path, error.Error);
                    }
                    else
                    {
                        typedValue = json;
                    }
                }
            }
        }
        catch
        {
            context.AddError(context.Path, T.Get("contents.validation.invalid"));
            return;
        }

        validator.Validate(typedValue, context);
    }
}
