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
using Squidex.Infrastructure.Geocoding;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public static class ContentValidationExtensions
    {
        public static async Task ValidateAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors, IGeocoder geocoder)
        {
            var validator = new ContentValidator(schema, partitionResolver, context, geocoder);

            await validator.ValidateAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidatePartialAsync(this NamedContentData data, ValidationContext context, Schema schema, PartitionResolver partitionResolver, IList<ValidationError> errors, IGeocoder geocoder)
        {
            var validator = new ContentValidator(schema, partitionResolver, context, geocoder);

            await validator.ValidatePartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }
    }
}
