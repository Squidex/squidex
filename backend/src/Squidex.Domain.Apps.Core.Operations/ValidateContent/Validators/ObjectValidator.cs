// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class ObjectValidator<TValue>(IDictionary<string, (bool IsOptional, IValidator Validator)> fields, bool isPartial, string fieldType) : IValidator
{
    private static readonly IReadOnlyDictionary<string, TValue> DefaultValue = new Dictionary<string, TValue>();

    public void Validate(object? value, ValidationContext context)
    {
        var originalValue = value;

        if (value.IsNullOrUndefined())
        {
            value = DefaultValue;
        }

        if (value is not IReadOnlyDictionary<string, TValue> values)
        {
            return;
        }

        foreach (var fieldData in values)
        {
            var name = fieldData.Key;

            if (!fields.ContainsKey(name))
            {
                context.AddError(T.Get("contents.validation.unknownField", new { fieldType }), context.Path.Enqueue(name));
            }
        }

        foreach (var (name, field) in fields)
        {
            var fieldValue = Undefined.Value;

            if (!values.TryGetValue(name, out var nestedValue))
            {
                // If the original value was unset, we have to validate the children for required values.
                if (isPartial && !originalValue.IsUnset())
                {
                    continue;
                }
            }
            else
            {
                fieldValue = nestedValue!;
            }

            // Use a special null values for unsets so we can treat them as null for required validators.
            if (Updates.IsUnset(fieldValue))
            {
                fieldValue = Undefined.Unset;
            }

            var fieldContext = context.Nested(name, field.IsOptional);

            field.Validator.Validate(fieldValue, fieldContext);
        }
    }
}
