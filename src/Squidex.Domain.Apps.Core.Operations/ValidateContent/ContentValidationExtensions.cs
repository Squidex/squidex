// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
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

        public static async Task ValidatePartialAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, partitionResolver, context);

            await validator.ValidatePartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }
    }
}
