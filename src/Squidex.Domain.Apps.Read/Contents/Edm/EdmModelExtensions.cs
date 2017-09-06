// ==========================================================================
//  EdmModelExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Squidex.Domain.Apps.Read.Contents.Edm
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
    }
}
