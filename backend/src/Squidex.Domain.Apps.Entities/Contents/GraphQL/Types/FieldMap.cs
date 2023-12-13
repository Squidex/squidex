// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class FieldMap
{
    private readonly Dictionary<string, Dictionary<string, string>> schemas = [];

    public FieldMap(IEnumerable<SchemaInfo> source)
    {
        foreach (var schema in source)
        {
            var fieldMap = schema.Fields.ToDictionary(x => x.FieldName, x => x.Field.Name);

            schemas[schema.Schema.Id.ToString()] = fieldMap;
            schemas[schema.Schema.Name] = fieldMap;
        }
    }

    public ContentData MapData(IInputObjectGraphType inputType, IDictionary<string, object?> source)
    {
        var result = new ContentData();

        foreach (var field in inputType.Fields)
        {
            if (field.ResolvedType is IComplexGraphType complexType && source.TryGetValue(field.Name, out var value) && value is IDictionary<string, object> nested)
            {
                result[field.SourceName()] = MapField(nested, complexType);
            }
        }

        return result;
    }

    public JsonValue MapNested(IInputObjectGraphType inputType, IDictionary<string, object?> source)
    {
        var result = new JsonObject(source.Count);

        foreach (var field in inputType.Fields)
        {
            if (source.TryGetValue(field.Name, out var value))
            {
                result[field.SourceName()] = MapValue(JsonGraphType.ParseJson(value));
            }
        }

        return result;
    }

    private ContentFieldData MapField(IDictionary<string, object> source, IComplexGraphType type)
    {
        var result = new ContentFieldData();

        foreach (var field in type.Fields)
        {
            if (source.TryGetValue(field.Name, out var value))
            {
                result[field.SourceName()] = MapValue(JsonGraphType.ParseJson(value));
            }
        }

        return result;
    }

    private JsonValue MapValue(JsonValue source)
    {
        switch (source.Value)
        {
            case JsonArray arr:
                return MapArray(arr);
            case JsonObject obj:
                return MapObject(obj);
            default:
                return source;
        }
    }

    private JsonValue MapArray(JsonArray source)
    {
        var result = new JsonArray(source.Count);

        foreach (var value in source)
        {
            result.Add(MapValue(value));
        }

        return result;
    }

    private JsonValue MapObject(JsonObject source)
    {
        Dictionary<string, string>? fieldMap = null;

        if (source.TryGetValue(Component.Discriminator, out var d1) && d1.Value is string discriminator)
        {
            schemas.TryGetValue(discriminator, out fieldMap);
        }
        else if (source.TryGetValue(Component.Descriptor, out var d2) && d2.Value is string descriptor)
        {
            schemas.TryGetValue(descriptor, out fieldMap);
        }

        if (fieldMap == null)
        {
            return source;
        }

        var result = new JsonObject(source.Count);

        foreach (var (key, value) in source)
        {
            var sourceName = key;

            if (fieldMap != null && fieldMap.TryGetValue(key, out var name))
            {
                sourceName = name;
            }

            result[sourceName] = MapValue(value);
        }

        return result;
    }
}
