// ==========================================================================
//  ContentExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Core
{
    public static class ContentExtensions
    {
        public static ContentData Enrich(this ContentData data, Schema schema, HashSet<Language> languages)
        {
            var enricher = new ContentEnricher(languages, schema);

            enricher.Enrich(data);

            return data;
        }

        public static async Task ValidateAsync(this ContentData data, Schema schema, HashSet<Language> languages, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, languages);

            await validator.ValidateAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidatePartialAsync(this ContentData data, Schema schema, HashSet<Language> languages, IList<ValidationError> errors)
        {
            var validator = new ContentValidator(schema, languages);

            await validator.ValidatePartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }
    }
}
