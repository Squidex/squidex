// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds;

internal static class ReferencesExtractor
{
    public record struct Args(JsonValue Value, ISet<DomainId> Result, int Take, ResolvedComponents Components);

    public static void Extract(IField field, JsonValue value, HashSet<DomainId> result, int take, ResolvedComponents components)
    {
        var args = new Args(value, result, take, components);

        ExtractCore(field, args);
    }

    public static void Extract(IEnumerable<IField> schema, ContentData data, HashSet<DomainId> result, int take, ResolvedComponents components)
    {
        foreach (var field in schema)
        {
            Extract(field, data, result, take, components);
        }
    }

    public static void Extract(IField field, ContentData data, HashSet<DomainId> result, int take, ResolvedComponents components)
    {
        if (CanHaveReferences(field.RawProperties) && data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
        {
            foreach (var (_, value) in fieldData)
            {
                Extract(field, value, result, take, components);
            }
        }
    }

    private static void ExtractCore(IField field, Args args)
    {
        switch (field)
        {
            case IField<AssetsFieldProperties>:
                AddIds(ref args);
                break;
            case IField<ReferencesFieldProperties>:
                AddIds(ref args);
                break;
            case IField<ComponentFieldProperties>:
                ExtractFromComponent(args.Value, args);
                break;
            case IField<ComponentsFieldProperties>:
                ExtractFromComponents(args);
                break;
            case IArrayField arrayField:
                ExtractFromArray(arrayField, args);
                break;
        }
    }

    private static void ExtractFromArray(IArrayField field, Args args)
    {
        if (args.Value.Value is JsonArray a)
        {
            foreach (var value in a)
            {
                ExtractFromItem(field, value, args);
            }
        }
    }

    private static void ExtractFromComponents(Args args)
    {
        if (args.Value.Value is JsonArray a)
        {
            foreach (var value in a)
            {
                ExtractFromComponent(value, args);
            }
        }
    }

    private static void ExtractFromItem(IArrayField field, JsonValue value, Args args)
    {
        if (value.Value is JsonObject o)
        {
            foreach (var nestedField in field.Fields)
            {
                if (CanHaveReferences(nestedField.RawProperties) && o.TryGetValue(nestedField.Name, out var nested))
                {
                    ExtractCore(nestedField, args with { Value = nested });
                }
            }
        }
    }

    private static void ExtractFromComponent(JsonValue value, Args args)
    {
        if (value.Value is JsonObject o)
        {
            if (o.TryGetValue(Component.Discriminator, out var found) && found.Value is string s)
            {
                var id = DomainId.Create(s);

                if (args.Components.TryGetValue(id, out var schema))
                {
                    foreach (var componentField in schema.Fields)
                    {
                        if (CanHaveReferences(componentField.RawProperties) && o.TryGetValue(componentField.Name, out var nested))
                        {
                            ExtractCore(componentField, args with { Value = nested });
                        }
                    }
                }
            }
        }
    }

    private static bool CanHaveReferences(FieldProperties properties)
    {
        return properties is ArrayFieldProperties or ReferencesFieldProperties or AssetsFieldProperties or ComponentFieldProperties or ComponentsFieldProperties;
    }

    private static void AddIds(ref Args args)
    {
        var added = 0;

        if (args.Value.Value is JsonArray a)
        {
            foreach (var id in a)
            {
                if (id.Value is string s)
                {
                    args.Result.Add(DomainId.Create(s));

                    added++;

                    if (added >= args.Take)
                    {
                        break;
                    }
                }
            }
        }
    }
}
