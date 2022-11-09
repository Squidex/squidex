// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using NamedIdStatic = Squidex.Infrastructure.NamedId;

namespace Squidex.Domain.Apps.Core.Schemas;

public static class FieldExtensions
{
    public static NamedId<long> NamedId(this IField field)
    {
        return NamedIdStatic.Of(field.Id, field.Name);
    }

    public static IEnumerable<T> ForApi<T>(this IEnumerable<T> fields, bool withHidden = false) where T : IField
    {
        return fields.Where(x => IsForApi(x, withHidden));
    }

    public static bool IsForApi<T>(this T field, bool withHidden = false) where T : IField
    {
        return (withHidden || !field.IsHidden) && !field.RawProperties.IsUIProperty();
    }

    public static bool IsComponentLike<T>(this T field) where T : IField
    {
        return field.RawProperties is ComponentFieldProperties or ComponentsFieldProperties;
    }

    public static bool IsUI<T>(this T field) where T : IField
    {
        return field.RawProperties is UIFieldProperties;
    }

    public static bool IsUIProperty<T>(this T properties) where T : FieldProperties
    {
        return properties is UIFieldProperties;
    }

    public static Schema ReorderFields(this Schema schema, List<long> ids, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.ReorderFields(ids);
                }

                return field;
            });
        }

        return schema.ReorderFields(ids);
    }

    public static Schema DeleteField(this Schema schema, long fieldId, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.DeleteField(fieldId);
                }

                return field;
            });
        }

        return schema.DeleteField(fieldId);
    }

    public static Schema LockField(this Schema schema, long fieldId, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.UpdateField(fieldId, f => f.Lock());
                }

                return field;
            });
        }

        return schema.UpdateField(fieldId, f => f.Lock());
    }

    public static Schema HideField(this Schema schema, long fieldId, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, f =>
            {
                if (f is ArrayField arrayField)
                {
                    return arrayField.UpdateField(fieldId, n => n.Hide());
                }

                return f;
            });
        }

        return schema.UpdateField(fieldId, f => f.Hide());
    }

    public static Schema ShowField(this Schema schema, long fieldId, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.UpdateField(fieldId, f => f.Show());
                }

                return field;
            });
        }

        return schema.UpdateField(fieldId, f => f.Show());
    }

    public static Schema EnableField(this Schema schema, long fieldId, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.UpdateField(fieldId, f => f.Enable());
                }

                return field;
            });
        }

        return schema.UpdateField(fieldId, f => f.Enable());
    }

    public static Schema DisableField(this Schema schema, long fieldId, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.UpdateField(fieldId, f => f.Disable());
                }

                return field;
            });
        }

        return schema.UpdateField(fieldId, f => f.Disable());
    }

    public static Schema UpdateField(this Schema schema, long fieldId, FieldProperties properties, long? parentId = null)
    {
        if (parentId != null)
        {
            return schema.UpdateField(parentId.Value, field =>
            {
                if (field is ArrayField arrayField)
                {
                    return arrayField.UpdateField(fieldId, f => f.Update(properties));
                }

                return field;
            });
        }

        return schema.UpdateField(fieldId, f => f.Update(properties));
    }
}
