// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Microsoft.OData.UriParser;

namespace Squidex.Infrastructure.MongoDb.OData
{
    public delegate string ConvertProperty(string[] parts);

    public static class PropertyBuilder
    {
        private static readonly ConvertProperty Default = parts =>
        {
            return string.Join(".", parts).ToPascalCase();
        };

        public static string BuildFieldDefinition(this QueryNode node, ConvertProperty convertProperty)
        {
            convertProperty = convertProperty ?? Default;

            var propertyParts = node.Accept(PropertyNameVisitor.Instance).ToArray();
            var propertyName = convertProperty(propertyParts);

            return propertyName;
        }
    }
}
