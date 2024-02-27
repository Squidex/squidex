// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Schemas;

public static class SchemaExtensions
{
    public static NamedId<DomainId> NamedId(this Schema schema)
    {
        return new NamedId<DomainId>(schema.Id, schema.Name);
    }

    public static long MaxId(this Schema schema)
    {
        var id = 0L;

        foreach (var field in schema.Fields)
        {
            if (field is IArrayField arrayField)
            {
                foreach (var nestedField in arrayField.Fields)
                {
                    id = Math.Max(id, nestedField.Id);
                }
            }

            id = Math.Max(id, field.Id);
        }

        return id;
    }

    public static string TypeName(this IField field)
    {
        return field.Name.ToPascalCase();
    }

    public static string DisplayName(this IField field)
    {
        return field.RawProperties.Label.Or(field.TypeName());
    }

    public static string TypeName(this Schema schema)
    {
        return schema.Name.ToPascalCase();
    }

    public static string DisplayName(this Schema schema)
    {
        return schema.Properties.Label.Or(schema.Name);
    }

    public static IEnumerable<RootField> ReferenceFields(this Schema schema)
    {
        return schema.RootFields(schema.FieldsInReferences);
    }

    public static IEnumerable<RootField> RootFields(this Schema schema, FieldNames names)
    {
        var hasField = false;

        foreach (var name in names)
        {
            if (!FieldNames.IsDataField(name, out var dataField))
            {
                continue;
            }

            if (schema.FieldsByName.TryGetValue(dataField, out var field))
            {
                hasField = true;

                yield return field;
            }
        }

        if (!hasField)
        {
            var first = schema.Fields.FirstOrDefault(x => !x.IsUI());

            if (first != null)
            {
                yield return first;
            }
        }
    }

    public static IEnumerable<IField<ReferencesFieldProperties>> ResolvingReferences(this Schema schema)
    {
        return schema.Fields.OfType<IField<ReferencesFieldProperties>>()
            .Where(x => x.Properties.ResolveReference && x.Properties.MaxItems == 1);
    }

    public static IEnumerable<IField<AssetsFieldProperties>> ResolvingAssets(this Schema schema)
    {
        return schema.Fields.OfType<IField<AssetsFieldProperties>>()
            .Where(x => x.Properties.ResolveFirst);
    }
}
