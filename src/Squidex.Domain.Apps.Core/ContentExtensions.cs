// ==========================================================================
//  ContentExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public static class ContentExtensions
    {
        public static void Enrich<T>(this ContentData<T> data, Schema schema, PartitionResolver partitionResolver)
        {
            var enricher = new ContentEnricher<T>(schema, partitionResolver);

            enricher.Enrich(data);
        }

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
