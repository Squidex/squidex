// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using NamedIdStatic = Squidex.Infrastructure.NamedId;

namespace Squidex.Domain.Apps.Core.Schemas
{
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

        public static IEnumerable<IRootField> GetSharedFields(this ResolvedComponents components, ImmutableList<DomainId>? schemaIds, bool withHidden)
        {
            if (schemaIds == null || schemaIds.Count == 0)
            {
                return Enumerable.Empty<IRootField>();
            }

            var allFields =
                schemaIds
                    .Select(x => components.Get(x)).NotNull()
                    .SelectMany(x => x.Fields.ForApi(withHidden))
                    .GroupBy(x => new { x.Name, Type = x.RawProperties.GetType() }).Where(x => x.Count() == 1)
                    .Select(x => x.First());

            return allFields;
        }

        public static bool IsForApi<T>(this T field, bool withHidden = false) where T : IField
        {
            return (withHidden || !field.IsHidden) && !field.RawProperties.IsUIProperty();
        }

        public static bool IsComponentLike<T>(this T field) where T : IField
        {
            return field.RawProperties is ComponentFieldProperties || field.RawProperties is ComponentsFieldProperties;
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
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.ReorderFields(ids);
                    }

                    return f;
                });
            }

            return schema.ReorderFields(ids);
        }

        public static Schema DeleteField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.DeleteField(fieldId);
                    }

                    return f;
                });
            }

            return schema.DeleteField(fieldId);
        }

        public static Schema LockField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Lock());
                    }

                    return f;
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
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Show());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Show());
        }

        public static Schema EnableField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Enable());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Enable());
        }

        public static Schema DisableField(this Schema schema, long fieldId, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Disable());
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Disable());
        }

        public static Schema UpdateField(this Schema schema, long fieldId, FieldProperties properties, long? parentId = null)
        {
            if (parentId != null)
            {
                return schema.UpdateField(parentId.Value, f =>
                {
                    if (f is ArrayField arrayField)
                    {
                        return arrayField.UpdateField(fieldId, n => n.Update(properties));
                    }

                    return f;
                });
            }

            return schema.UpdateField(fieldId, f => f.Update(properties));
        }
    }
}
