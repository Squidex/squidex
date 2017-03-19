// ==========================================================================
//  FieldExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Core
{
    public static class FieldExtensions
    {
        public static async Task ValidateAsync(this Field field, JToken value, Action<string> addError)
        {
            Guard.NotNull(value, nameof(value));

            try
            {
                var typedValue = value.IsNull() ? null : field.ConvertValue(value);

                foreach (var validator in field.Validators)
                {
                    await validator.ValidateAsync(typedValue, addError);
                }
            }
            catch
            {
                addError("<FIELD> is not a valid value");
            }
        }
    }
}
