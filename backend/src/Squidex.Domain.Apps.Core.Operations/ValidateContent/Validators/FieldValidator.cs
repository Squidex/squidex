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

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
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

        public async ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            var typedValue = value;

            try
            {
                if (value is JsonValue jsonValue)
                {
                    if (jsonValue.Type == JsonValueType.Null)
                    {
                        typedValue = null;
                    }
                    else
                    {
                        typedValue = jsonValue.RawValue;

                        var (json, error) = JsonValueConverter.ConvertValue(field, jsonValue, context.JsonSerializer, context.Components);

                        if (error != null)
                        {
                            addError(context.Path, error.Error);
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
                addError(context.Path, T.Get("contents.validation.invalid"));
                return;
            }

            await validator.ValidateAsync(typedValue, context, addError);
        }
    }
}
