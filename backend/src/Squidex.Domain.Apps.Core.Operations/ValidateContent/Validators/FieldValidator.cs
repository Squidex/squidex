﻿// ==========================================================================
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

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class FieldValidator : IValidator
    {
        private readonly IValidator[] validators;
        private readonly IField field;

        public FieldValidator(IEnumerable<IValidator> validators, IField field)
        {
            Guard.NotNull(field);

            this.validators = validators.ToArray();

            this.field = field;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            try
            {
                var typedValue = value;

                if (value is IJsonValue jsonValue)
                {
                    if (jsonValue.Type == JsonValueType.Null)
                    {
                        typedValue = null;
                    }
                    else
                    {
                        typedValue = JsonValueConverter.ConvertValue(field, jsonValue);
                    }
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
            catch
            {
                addError(context.Path, "Not a valid value.");
            }
        }
    }
}
