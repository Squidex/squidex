// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Tasks;
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

        public async ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError)
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

                await AsyncHelper.WhenAllThrottledAsync(fields, async (kvp, _) =>
                {
                    var (isOptional, validator) = kvp.Value;

                    var fieldValue = Undefined.Value;

                    if (!values.TryGetValue(kvp.Key, out var nestedValue))
                    {
                        if (isPartial)
                        {
                            return;
                        }
                    }
                    else
                    {
                        fieldValue = nestedValue!;
                    }

                    var fieldContext = context.Nested(kvp.Key).Optional(isOptional);

                    await validator.ValidateAsync(fieldValue, fieldContext, addError);
                });
            }
        }
    }
}
