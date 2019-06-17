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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors
{
    public static class FilterFactory
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        private static readonly Dictionary<string, string> PropertyMap =
            typeof(MongoContentEntity).GetProperties()
                .ToDictionary(x => x.Name, x => x.GetCustomAttribute<BsonElementAttribute>()?.ElementName ?? x.Name, StringComparer.OrdinalIgnoreCase);

        public static Query AdjustToModel(this Query query, Schema schema, bool useDraft)
        {
            var pathConverter = PathConverter(schema, useDraft);

            if (query.Filter != null)
            {
                query.Filter = query.Filter.Accept(new AdaptionVisitor(pathConverter));
            }

            query.Sort = query.Sort.Select(x => new SortNode(pathConverter(x.Path), x.SortOrder)).ToList();

            return query;
        }

        public static FilterNode AdjustToModel(this FilterNode filterNode, Schema schema, bool inDraft)
        {
            var pathConverter = PathConverter(schema, inDraft);

            return filterNode.Accept(new AdaptionVisitor(pathConverter));
        }

        private static Func<IReadOnlyList<string>, IReadOnlyList<string>> PathConverter(Schema schema, bool inDraft)
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
                        if (inDraft)
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
            };
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

        public static IFindFluent<MongoContentEntity, MongoContentEntity> WithoutDraft(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, bool includeDraft)
        {
            return !includeDraft ? cursor.Not(x => x.DataDraftByIds, x => x.IsDeleted) : cursor;
        }

        public static FilterDefinition<MongoContentEntity> Build(Guid schemaId, Guid id, Status[] status)
        {
            return CreateFilter(null, schemaId, new List<Guid> { id }, status, null);
        }

        public static FilterDefinition<MongoContentEntity> IdsByApp(Guid appId, ICollection<Guid> ids, Status[] status)
        {
            return CreateFilter(appId, null, ids, status, null);
        }

        public static FilterDefinition<MongoContentEntity> IdsBySchema(Guid schemaId, ICollection<Guid> ids, Status[] status)
        {
            return CreateFilter(null, schemaId, ids, status, null);
        }

        public static FilterDefinition<MongoContentEntity> ToFilter(this Query query, Guid schemaId, ICollection<Guid> ids, Status[] status)
        {
            return CreateFilter(null, schemaId, ids, status, query);
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(Guid? appId, Guid? schemaId, ICollection<Guid> ids, Status[] status, Query query)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>();

            if (appId.HasValue)
            {
                filters.Add(Filter.Eq(x => x.IndexedAppId, appId.Value));
            }

            if (schemaId.HasValue)
            {
                filters.Add(Filter.Eq(x => x.IndexedSchemaId, schemaId.Value));
            }

            filters.Add(Filter.Ne(x => x.IsDeleted, true));

            if (status != null)
            {
                filters.Add(Filter.In(x => x.Status, status));
            }

            if (ids != null && ids.Count > 0)
            {
                if (ids.Count > 1)
                {
                    filters.Add(Filter.In(x => x.Id, ids));
                }
                else
                {
                    filters.Add(Filter.Eq(x => x.Id, ids.First()));
                }
            }

            if (query?.Filter != null)
            {
                filters.Add(query.Filter.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }

        public static FilterDefinition<MongoContentEntity> ToFilter(this FilterNode filterNode, Guid schemaId)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true),
                filterNode.BuildFilter<MongoContentEntity>()
            };

            return Filter.And(filters);
        }
    }
}
