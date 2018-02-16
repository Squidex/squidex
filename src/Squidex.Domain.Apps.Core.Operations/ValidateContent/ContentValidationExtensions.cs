// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public static class ContentValidationExtensions
    {
        public static async Task ValidateAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, partitionResolver, context);

            await validator.ValidateAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidateAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, Func<string> message)
        {
            var validator = new ContentValidator(schema, partitionResolver, context);

            await validator.ValidateAsync(data);

            if (validator.Errors.Count > 0)
            {
                throw new ValidationException(message(), validator.Errors.ToList());
            }
        }

        public static async Task ValidatePartialAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, partitionResolver, context);

            await validator.ValidatePartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidatePartialAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, Func<string> message)
        {
            var validator = new ContentValidator(schema, partitionResolver, context);

            await validator.ValidatePartialAsync(data);

            if (validator.Errors.Count > 0)
            {
                throw new ValidationException(message(), validator.Errors.ToList());
            }
        }
    }
}
