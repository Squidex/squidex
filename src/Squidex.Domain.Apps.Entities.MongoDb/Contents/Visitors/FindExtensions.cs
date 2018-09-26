// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.MongoDb.OData;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors
{
    public static class FindExtensions
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        private static readonly Dictionary<string, string> PropertyMap =
            typeof(MongoContentEntity).GetProperties()
                .ToDictionary(x => x.Name, x => x.GetCustomAttribute<BsonElementAttribute>()?.ElementName ?? x.Name, StringComparer.OrdinalIgnoreCase);

        private sealed class AdaptionVisitor : TransformVisitor
        {
            private readonly Func<IReadOnlyList<string>, IReadOnlyList<string>> pathConverter;

            public AdaptionVisitor(Func<IReadOnlyList<string>, IReadOnlyList<string>> pathConverter)
            {
                this.pathConverter = pathConverter;
            }

            public override FilterNode Visit(FilterComparison nodeIn)
            {
                var value = nodeIn.Rhs.Value;

                if (value is Instant instant &&
                    !string.Equals(nodeIn.Lhs[0], "mt", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(nodeIn.Lhs[0], "ct", StringComparison.OrdinalIgnoreCase))
                {
                    return new FilterComparison(pathConverter(nodeIn.Lhs), nodeIn.Operator, new FilterValue(value.ToString()));
                }

                return new FilterComparison(pathConverter(nodeIn.Lhs), nodeIn.Operator, nodeIn.Rhs);
            }
        }

        public static Query AdjustToModel(this Query query, Schema schema, bool useDraft)
        {
            var pathConverter = new Func<IReadOnlyList<string>, IReadOnlyList<string>>(propertyNames =>
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
                }

                if (result.Count > 0)
                {
                    if (result[0].Equals("Data", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (useDraft)
                        {
                            result[0] = "dd";
                        }
                        else
                        {
                            result[0] = "do";
                        }
                    }
                    else
                    {
                        result[0] = PropertyMap[propertyNames[0]];
                    }
                }

                return result;
            });

            if (query.Filter != null)
            {
                query.Filter = query.Filter.Accept(new AdaptionVisitor(pathConverter));
            }

            query.Sort = query.Sort.Select(x => new SortNode(pathConverter(x.Path), x.SortOrder)).ToList();

            return query;
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentSort(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, Query query)
        {
            return cursor.Sort(query.BuildSort<MongoContentEntity>());
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentTake(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, Query query)
        {
            return cursor.Take(query);
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentSkip(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, Query query)
        {
            return cursor.Skip(query);
        }

        public static FilterDefinition<MongoContentEntity> BuildQuery(Query query, Guid schemaId, Status[] status)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedSchemaId, schemaId)
            };

            if (status != null)
            {
                filters.Add(Filter.Ne(x => x.IsDeleted, true));
                filters.Add(Filter.In(x => x.Status, status));
            }

            var filter = query.BuildFilter<MongoContentEntity>();

            if (filter.Filter != null)
            {
                if (filter.Last)
                {
                    filters.Add(filter.Filter);
                }
                else
                {
                    filters.Insert(0, filter.Filter);
                }
            }

            if (filters.Count == 1)
            {
                return filters[0];
            }
            else
            {
                return Filter.And(filters);
            }
        }
    }
}
