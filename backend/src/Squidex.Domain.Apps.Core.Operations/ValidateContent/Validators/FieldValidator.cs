// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class FieldValidator : IValidator
    {
        private readonly IValidator fieldValueValidator;
        private readonly IField field;

        public FieldValidator(IValidator fieldValueValidator, IField field)
        {
            Guard.NotNull(field, nameof(field));
            Guard.NotNull(fieldValueValidator, nameof(fieldValueValidator));

            this.field = field;
            this.fieldValueValidator = fieldValueValidator;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            var typedValue = value;

            try
            {
                if (value is IJsonValue jsonValue)
                {
                    if (jsonValue.Type == JsonValueType.Null)
                    {
                        typedValue = null;
                    }
                    else
                    {
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

            await fieldValueValidator.ValidateAsync(typedValue, context, addError);
        }
    }
}
