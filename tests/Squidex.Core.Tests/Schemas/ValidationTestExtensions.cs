// ==========================================================================
//  ValidationTestExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas.Validators;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Core.Schemas
{
    public static class ValidationTestExtensions
    {
        private static readonly ValidationContext EverythingValidContext = new ValidationContext((x, y) => TaskHelper.True, x => TaskHelper.True);

        public static Task ValidateAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value, context ?? EverythingValidContext, errors.Add);
        }

        public static Task ValidateOptionalAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value, (context ?? EverythingValidContext).Optional(true), errors.Add);
        }

        public static Task ValidateAsync(this Field field, JToken value, IList<string> errors, ValidationContext context = null)
        {
            return field.ValidateAsync(value, context ?? EverythingValidContext, errors.Add);
        }

        public static Task ValidateOptionalAsync(this Field field, JToken value, IList<string> errors, ValidationContext context = null)
        {
            return field.ValidateAsync(value, (context ?? EverythingValidContext).Optional(true), errors.Add);
        }
    }
}
