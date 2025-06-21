// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
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
        var typedValue = ResolveValue(value, context);

        validator.Validate(typedValue, context);
    }

    private object? ResolveValue(object? value, ValidationContext context)
    {
        if (value is not JsonValue jsonValue)
        {
            return value;
        }

        if (jsonValue == default || Updates.IsUnset(jsonValue))
        {
            return null;
        }

        var result = jsonValue.Value;
        try
        {
            var (typed, error) = JsonValueConverter.ConvertValue(field, jsonValue,
                context.Root.Serializer,
                context.Root.Components);

            if (error != null)
            {
                context.AddError(error.Error);
            }
            else
            {
                result = typed;
            }
        }
        catch
        {
            context.AddError(T.Get("contents.validation.invalid"));
        }

        return result;
    }
}
