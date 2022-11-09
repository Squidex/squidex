// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.OData;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

public static class Adapt
{
    private static Dictionary<string, PropertyPath> pathMap;
    private static Dictionary<string, string> propertyMap;

    public static IReadOnlyDictionary<string, string> PropertyMap
    {
        get
        {
            if (propertyMap == null)
            {
                propertyMap =
                    BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).AllMemberMaps
                        .ToDictionary(
                            x => x.MemberName,
                            x => x.ElementName,
                            StringComparer.OrdinalIgnoreCase);
            }

            return propertyMap;
        }
    }

    public static IReadOnlyDictionary<string, PropertyPath> PathMap
    {
        get
        {
            if (pathMap == null)
            {
                pathMap = PropertyMap.ToDictionary(x => x.Key, x => (PropertyPath)x.Value);
            }

            return pathMap;
        }
    }

    public static PropertyPath MapPath(PropertyPath path)
    {
        if (path.Count == 1 && PathMap.TryGetValue(path[0], out var mappedPath))
        {
            return mappedPath;
        }

        var result = new List<string>(path);

        if (result.Count > 0)
        {
            if (PropertyMap.TryGetValue(path[0], out var mapped))
            {
                result[0] = mapped;
            }
        }

        for (var i = 1; i < path.Count; i++)
        {
            result[i] = result[i].UnescapeEdmField().JsonToBsonName().JsonEscape();
        }

        return result;
    }

    public static ClrQuery AdjustToModel(this ClrQuery query, DomainId appId)
    {
        if (query.Filter != null)
        {
            query.Filter = AdaptionVisitor.AdaptFilter(query.Filter);
        }

        if (query.Filter != null)
        {
            query.Filter = AdaptIdVisitor.AdaptFilter(query.Filter, appId);
        }

        if (query.Sort != null)
        {
            query.Sort = query.Sort.Select(x => new SortNode(MapPath(x.Path), x.Order)).ToList();
        }

        return query;
    }

    public static FilterNode<ClrValue>? AdjustToModel(this FilterNode<ClrValue> filter, DomainId appId)
    {
        var result = AdaptionVisitor.AdaptFilter(filter);

        if (result != null)
        {
            result = AdaptIdVisitor.AdaptFilter(result, appId);
        }

        return result;
    }
}
