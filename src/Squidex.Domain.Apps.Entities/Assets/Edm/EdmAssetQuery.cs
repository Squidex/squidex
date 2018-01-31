// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Edm
{
    public static class EdmAssetQuery
    {
        public static ODataUriParser ParseQuery(string query)
        {
            try
            {
                var model = EdmAssetModel.Edm;

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
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }
    }
}
