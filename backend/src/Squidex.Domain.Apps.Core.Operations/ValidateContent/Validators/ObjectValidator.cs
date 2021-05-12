// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class ObjectValidator<TValue> : IValidator
    {
        private static readonly IReadOnlyDictionary<string, TValue> DefaultValue = new Dictionary<string, TValue>();
        private readonly IDictionary<string, (bool IsOptional, IValidator Validator)> fields;
        private readonly bool isPartial;
        private readonly string fieldType;

        public ObjectValidator(IDictionary<string, (bool IsOptional, IValidator Validator)> fields, bool isPartial, string fieldType)
        {
            this.fields = fields;
            this.fieldType = fieldType;

            this.isPartial = isPartial;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value.IsNullOrUndefined())
            {
                value = DefaultValue;
            }

            if (value is IReadOnlyDictionary<string, TValue> values)
            {
                foreach (var fieldData in values)
                {
                    var name = fieldData.Key;

                    if (!fields.ContainsKey(name))
                    {
                        addError(context.Path.Enqueue(name), T.Get("contents.validation.unknownField", new { fieldType }));
                    }
                }

                var tasks = new List<Task>();

                foreach (var (fieldName, fieldConfig) in fields)
                {
                    var (isOptional, validator) = fieldConfig;

                    var fieldValue = Undefined.Value;

                    if (!values.TryGetValue(fieldName, out var nestedValue))
                    {
                        if (isPartial)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        fieldValue = nestedValue!;
                    }

                    var fieldContext = context.Nested(fieldName).Optional(isOptional);

                    tasks.Add(validator.ValidateAsync(fieldValue, fieldContext, addError));
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
