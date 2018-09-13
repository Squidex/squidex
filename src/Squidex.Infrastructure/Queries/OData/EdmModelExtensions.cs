// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.Queries.OData
{
    public static class EdmModelExtensions
    {
        public static ODataUriParser ParseQuery(this IEdmModel model, string query)
        {
            if (!model.EntityContainer.EntitySets().Any())
            {
                return null;
            }

            query = query ?? string.Empty;

            var path = model.EntityContainer.EntitySets().First().Path.Path.Split('.').Last();

            if (query.StartsWith("?", StringComparison.Ordinal))
            {
                query = query.Substring(1);
            }

            var parser = new ODataUriParser(model, new Uri($"{path}?{query}", UriKind.Relative));

            return parser;
        }

        public static Query ToQuery(this ODataUriParser parser)
        {
            var query = new Query();

            if (parser != null)
            {
                parser.ParseTake(query);
                parser.ParseSkip(query);
                parser.ParseFilter(query);
                parser.ParseSort(query);
            }

            return query;
        }
    }
}
