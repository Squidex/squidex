// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public static class Adapt
    {
        private static readonly Dictionary<string, string> PropertyMap =
            typeof(MongoContentEntity).GetProperties()
                .ToDictionary(
                    x => ToElementName(x),
                    x => ToName(x),
                    StringComparer.OrdinalIgnoreCase);

        private static string ToName(PropertyInfo x)
        {
            return x.GetCustomAttribute<BsonElementAttribute>()?.ElementName ?? x.Name;
        }

        private static string ToElementName(PropertyInfo x)
        {
            return x.Name;
        }

        public static Func<PropertyPath, PropertyPath> Path(Schema schema)
        {
            return propertyNames =>
            {
                var result = new List<string>(propertyNames);

                if (result.Count > 1)
                {
                    var edmName = result[1].UnescapeEdmField();

                    if (!schema.FieldsByName.TryGetValue(edmName, out var field))
                    {
                        throw new NotSupportedException();
                    }

                    result[1] = field.Id.ToString();

                    if (field is IArrayField arrayField && result.Count > 3)
                    {
                        var nestedEdmName = result[3].UnescapeEdmField();

                        if (!arrayField.FieldsByName.TryGetValue(nestedEdmName, out var nestedField))
                        {
                            throw new NotSupportedException();
                        }

                        result[3] = nestedField.Id.ToString();
                    }
                }

                if (result.Count > 2)
                {
                    result[2] = result[2].UnescapeEdmField();
                }

                if (result.Count > 0)
                {
                    if (result[0].Equals("Data", StringComparison.CurrentCultureIgnoreCase))
                    {
                        result[0] = "do";
                    }
                    else
                    {
                        result[0] = PropertyMap[propertyNames[0]];
                    }
                }

                return result;
            };
        }

        public static ClrQuery AdjustToModel(this ClrQuery query, Schema schema)
        {
            var pathConverter = Path(schema);

            if (query.Filter != null)
            {
                query.Filter = query.Filter.Accept(new AdaptionVisitor(pathConverter));
            }

            query.Sort = query.Sort.Select(x => new SortNode(pathConverter(x.Path), x.Order)).ToList();

            return query;
        }

        public static FilterNode<ClrValue>? AdjustToModel(this FilterNode<ClrValue> filterNode, Schema schema)
        {
            var pathConverter = Path(schema);

            return filterNode.Accept(new AdaptionVisitor(pathConverter));
        }
    }
}
