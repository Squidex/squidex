// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors
{
    public static class FilterFactory
    {
        private static readonly FilterDefinitionBuilder<MongoContentEntity> Filter = Builders<MongoContentEntity>.Filter;

        public static ClrQuery AdjustToModel(this ClrQuery query, Schema schema, bool useDraft)
        {
            var pathConverter = Adapt.Path(schema, useDraft);

            if (query.Filter != null)
            {
                query.Filter = query.Filter.Accept(new AdaptionVisitor(pathConverter));
            }

            query.Sort = query.Sort.Select(x => new SortNode(pathConverter(x.Path), x.Order)).ToList();

            return query;
        }

        public static FilterNode<ClrValue>? AdjustToModel(this FilterNode<ClrValue> filterNode, Schema schema, bool useDraft)
        {
            var pathConverter = Adapt.Path(schema, useDraft);

            return filterNode.Accept(new AdaptionVisitor(pathConverter));
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentSort(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ClrQuery query)
        {
            return cursor.Sort(query.BuildSort<MongoContentEntity>());
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentTake(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ClrQuery query)
        {
            return cursor.Take(query);
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> ContentSkip(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, ClrQuery query)
        {
            return cursor.Skip(query);
        }

        public static IFindFluent<MongoContentEntity, MongoContentEntity> WithoutDraft(this IFindFluent<MongoContentEntity, MongoContentEntity> cursor, bool includeDraft)
        {
            return !includeDraft ? cursor.Not(x => x.DataDraftByIds, x => x.IsDeleted) : cursor;
        }

        public static FilterDefinition<MongoContentEntity> IdsByApp(Guid appId, ICollection<Guid> ids, Status[]? status)
        {
            return CreateFilter(appId, null, ids, status, null);
        }

        public static FilterDefinition<MongoContentEntity> IdsBySchema(Guid schemaId, ICollection<Guid> ids, Status[]? status)
        {
            return CreateFilter(null, schemaId, ids, status, null);
        }

        public static FilterDefinition<MongoContentEntity> ToFilter(this ClrQuery query, Guid schemaId, ICollection<Guid>? ids, Status[]? status)
        {
            return CreateFilter(null, schemaId, ids, status, query);
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(Guid? appId, Guid? schemaId, ICollection<Guid>? ids, Status[]? status,
            ClrQuery? query)
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

        public static FilterDefinition<MongoContentEntity> ToFilter(this FilterNode<ClrValue> filterNode, Guid schemaId)
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
