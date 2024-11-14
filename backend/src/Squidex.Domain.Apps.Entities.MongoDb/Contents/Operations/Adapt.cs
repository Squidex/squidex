// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using MongoDB.Bson.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.OData;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

public static class Adapt
{
    private static Dictionary<string, PropertyPath> pathMap;
    private static Dictionary<string, PropertyPath> pathReverseMap;
    private static Dictionary<string, string> propertyMap;
    private static Dictionary<string, string> propertyReverseMap;

    public static IReadOnlyDictionary<string, string> PropertyMap
    {
        get => propertyMap ??=
            BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).AllMemberMaps
                .ToDictionary(
                    x => x.MemberName,
                    x => x.ElementName,
                    StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, string> PropertyReverseMap
    {
        get => propertyReverseMap ??=
            BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).AllMemberMaps
                .ToDictionary(
                    x => x.ElementName,
                    x => x.MemberName.ToCamelCase(),
                    StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, PropertyPath> PathMap
    {
        get => pathMap ??= PropertyMap.ToDictionary(x => x.Key, x => (PropertyPath)x.Value);
    }

    public static IReadOnlyDictionary<string, PropertyPath> PathReverseMap
    {
        get => pathReverseMap ??= PropertyReverseMap.ToDictionary(x => x.Key, x => (PropertyPath)x.Value);
    }

    public static PropertyPath MapPath(PropertyPath path)
    {
        // Shortcut to prevent allocations for most used field names.
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
            // MongoDB does not accept all field names.
            result[i] = result[i].UnescapeEdmField().JsonToBsonName().JsonEscape();
        }

        return result;
    }

    public static PropertyPath MapPathReverse(PropertyPath path)
    {
        // Shortcut to prevent allocations for most used field names.
        if (path.Count == 1 && PathReverseMap.TryGetValue(path[0], out var mappedPath))
        {
            return mappedPath;
        }

        var result = new List<string>(path);

        if (result.Count > 0)
        {
            if (PropertyReverseMap.TryGetValue(path[0], out var mapped))
            {
                result[0] = mapped;
            }
        }

        for (var i = 1; i < path.Count; i++)
        {
            // MongoDB does not accept all field names.
            result[i] = result[i].EscapeEdmField().BsonToJsonName().JsonUnescape().ToCamelCase();
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
