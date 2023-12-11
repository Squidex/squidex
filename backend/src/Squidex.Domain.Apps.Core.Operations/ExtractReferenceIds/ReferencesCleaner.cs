// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds;

internal sealed class ReferencesCleaner : IFieldPropertiesVisitor<JsonValue, ReferencesCleaner.Args>
{
    private static readonly ReferencesCleaner Instance = new ReferencesCleaner();

    public record struct Args(JsonValue Value, ISet<DomainId> ValidIds);

    private ReferencesCleaner()
    {
    }

    public static JsonValue Cleanup(IField field, JsonValue value, HashSet<DomainId> validIds)
    {
        var args = new Args(value, validIds);

        return field.RawProperties.Accept(Instance, args);
    }

    public JsonValue Visit(ArrayFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(AssetsFieldProperties properties, Args args)
    {
        return CleanIds(args);
    }

    public JsonValue Visit(ReferencesFieldProperties properties, Args args)
    {
        return CleanIds(args);
    }

    public JsonValue Visit(BooleanFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(ComponentFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(ComponentsFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(DateTimeFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(GeolocationFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(JsonFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(NumberFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(RichTextFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(StringFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(TagsFieldProperties properties, Args args)
    {
        return args.Value;
    }

    public JsonValue Visit(UIFieldProperties properties, Args args)
    {
        return args.Value;
    }

    private static JsonValue CleanIds(Args args)
    {
        if (args.Value.Value is JsonArray array)
        {
            var result = args.Value.AsArray;

            for (var i = 0; i < result.Count; i++)
            {
                if (!IsValidReference(result[i], args))
                {
                    if (ReferenceEquals(result, array))
                    {
                        result = array;
                    }

                    result.RemoveAt(i);
                    i--;
                }
            }

            return result;
        }

        return args.Value;
    }

    private static bool IsValidReference(JsonValue item, Args args)
    {
        return item.Value is string s && args.ValidIds.Contains(DomainId.Create(s));
    }
}
