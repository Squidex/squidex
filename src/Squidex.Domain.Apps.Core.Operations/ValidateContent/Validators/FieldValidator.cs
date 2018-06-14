// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class FieldValidator : IValidator
    {
        private readonly IValidator[] validators;
        private readonly IField field;

        public FieldValidator(IValidator[] validators, IField field)
        {
            this.validators = validators;
            this.field = field;
        }

        public async Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            try
            {
                object typedValue = null;

                if (value is JToken jToken)
                {
                    typedValue = jToken.IsNull() ? null : JsonValueConverter.ConvertValue(field, jToken);
                }

                var tasks = new List<Task>();

                foreach (var validator in ValidatorsFactory.CreateValidators(field))
                {
                    tasks.Add(validator.ValidateAsync(typedValue, context, addError));
                }

                await Task.WhenAll(tasks);
            }
            catch
            {
                addError(context.Path, "Not a valid value.");
            }
        }
    }
}
