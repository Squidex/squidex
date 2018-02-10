// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Microsoft.OData.UriParser;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public delegate string PropertyCalculator(string[] parts);

    public static class PropertyBuilder
    {
        private static readonly PropertyCalculator DefaultCalculator = parts =>
        {
            return string.Join(".", parts).ToPascalCase();
        };

        public static StringFieldDefinition<T, object> BuildFieldDefinition<T>(this QueryNode node, PropertyCalculator propertyCalculator)
        {
            propertyCalculator = propertyCalculator ?? DefaultCalculator;

            var propertyParts = node.Accept(PropertyNameVisitor.Instance).ToArray();
            var propertyName = propertyCalculator(propertyParts);

            return new StringFieldDefinition<T, object>(propertyName);
        }
    }
}
