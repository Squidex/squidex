// ==========================================================================
//  ValidationTestExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas.Validators;

namespace Squidex.Core.Schemas
{
    public static class ValidationTestExtensions
    {
        private static readonly Task<IReadOnlyList<Guid>> ValidIds = Task.FromResult<IReadOnlyList<Guid>>(new List<Guid>());

        public static readonly ValidationContext ValidContext = new ValidationContext((x, y) => ValidIds, x => ValidIds);

        public static Task ValidateAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value, context ?? ValidContext, errors.Add);
        }

        public static Task ValidateOptionalAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value, (context ?? ValidContext).Optional(true), errors.Add);
        }

        public static Task ValidateAsync(this Field field, JToken value, IList<string> errors, ValidationContext context = null)
        {
            return field.ValidateAsync(value, context ?? ValidContext, errors.Add);
        }

        public static Task ValidateOptionalAsync(this Field field, JToken value, IList<string> errors, ValidationContext context = null)
        {
            return field.ValidateAsync(value, (context ?? ValidContext).Optional(true), errors.Add);
        }

        public static ValidationContext InvalidContext(Guid assetId)
        {
            var invalidIds = Task.FromResult<IReadOnlyList<Guid>>(new List<Guid> { assetId });

            return new ValidationContext((x, y) => invalidIds, x => invalidIds);
        }
    }
}
