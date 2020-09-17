// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class FieldValidator : IValidator
    {
        private readonly IValidator[]? validators;
        private readonly IField field;

        public FieldValidator(IEnumerable<IValidator>? validators, IField field)
        {
            Guard.NotNull(field, nameof(field));

            this.validators = validators?.ToArray();

            this.field = field;
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
                        var (json, error) = JsonValueConverter.ConvertValue(field, jsonValue);

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

            if (validators?.Length > 0)
            {
                var tasks = new List<Task>();

                foreach (var validator in validators)
                {
                    tasks.Add(validator.ValidateAsync(typedValue, context, addError));
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
