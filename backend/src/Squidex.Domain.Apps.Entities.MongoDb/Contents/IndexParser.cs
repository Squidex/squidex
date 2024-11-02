// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public static class IndexParser
{
    public static bool TryParse(BsonDocument source, string prefix, [MaybeNullWhen(false)] out IndexDefinition index)
    {
        index = null!;

        if (!source.TryGetValue("name", out var name) || name.BsonType != BsonType.String)
        {
            return false;
        }

        if (!name.AsString.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (!source.TryGetValue("key", out var keys) || keys.BsonType != BsonType.Document)
        {
            return false;
        }

        var definition = new IndexDefinition();
        foreach (var property in keys.AsBsonDocument)
        {
            if (property.Value.BsonType != BsonType.Int32)
            {
                return false;
            }

            var fieldName = Adapt.MapPathReverse(property.Name).ToString();

            var order = property.Value.AsInt32 < 0 ?
                SortOrder.Descending :
                SortOrder.Ascending;

            definition.Add(new IndexField(fieldName, order));
        }

        if (definition.Count == 0)
        {
            return false;
        }

        index = definition;
        return true;
    }
}
