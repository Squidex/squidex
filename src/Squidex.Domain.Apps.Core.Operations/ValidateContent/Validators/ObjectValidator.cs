// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class ObjectValidator<TValue> : IValidator
    {
        private readonly IDictionary<string, (bool IsOptional, IValidator Validator)> schema;
        private readonly bool isPartial;
        private readonly string fieldType;
        private readonly TValue fieldDefault;

        public ObjectValidator(IDictionary<string, (bool IsOptional, IValidator Validator)> schema, bool isPartial, string fieldType, TValue fieldDefault)
        {
            this.schema = schema;
            this.fieldDefault = fieldDefault;
            this.fieldType = fieldType;
            this.isPartial = isPartial;
        }

        public async Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value is IDictionary<string, TValue> values)
            {
                foreach (var fieldData in values)
                {
                    var name = fieldData.Key;

                    if (!schema.ContainsKey(name))
                    {
                        addError(context.Path.Enqueue(name), $"Not a known {fieldType}.");
                    }
                }

                var tasks = new List<Task>();

                foreach (var field in schema)
                {
                    var name = field.Key;

                    if (!values.TryGetValue(name, out var fieldValue))
                    {
                        if (isPartial)
                        {
                            continue;
                        }

                        fieldValue = fieldDefault;
                    }

                    var (isOptional, validator) = field.Value;
                    var fieldContext = context.Nested(name).Optional(isOptional);

                    tasks.Add(validator.ValidateAsync(fieldValue, fieldContext, addError));
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
