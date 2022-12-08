// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;

public static class FindExtensions
{
    private static readonly FilterDefinitionBuilder<MongoAssetEntity> Filter = Builders<MongoAssetEntity>.Filter;

    public static ClrQuery AdjustToModel(this ClrQuery query, DomainId appId)
    {
        if (query.Filter != null)
        {
            query.Filter = FirstPascalPathConverter<ClrValue>.Transform(query.Filter);
        }

        if (query.Filter != null)
        {
            query.Filter = AdaptIdVisitor.AdaptFilter(query.Filter, appId);
        }

        if (query.Sort != null)
        {
            query.Sort = query.Sort.Select(x => new SortNode(x.Path.ToFirstPascalCase(), x.Order)).ToList();
        }

        return query;
    }

    public static (FilterDefinition<MongoAssetEntity>, bool) BuildFilter(this ClrQuery query, DomainId appId, DomainId? parentId)
    {
        var filters = new List<FilterDefinition<MongoAssetEntity>>
        {
            Filter.Ne(x => x.LastModified, default),
            Filter.Ne(x => x.Id, default),
            Filter.Eq(x => x.IndexedAppId, appId)
        };

        var isDefault = false;

        if (!query.HasFilterField("IsDeleted"))
        {
            filters.Add(Filter.Eq(x => x.IsDeleted, false));

            isDefault = true;
        }

        if (parentId != null)
        {
            if (parentId == DomainId.Empty)
            {
                filters.Add(
                    Filter.Or(
                        Filter.Exists(x => x.ParentId, false),
                        Filter.Eq(x => x.ParentId, DomainId.Empty)));
            }
            else
            {
                filters.Add(Filter.Eq(x => x.ParentId, parentId.Value));
            }

            isDefault = false;
        }

        var (filter, last) = query.BuildFilter<MongoAssetEntity>(false);

        if (filter != null)
        {
            isDefault = false;

            if (last)
            {
                filters.Add(filter);
            }
            else
            {
                filters.Insert(0, filter);
            }
        }

        return (Filter.And(filters), isDefault);
    }
}
