// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
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

        public static IEnumerable<IRootField> GetSharedFields(this IField<ComponentFieldProperties> field, bool withHidden)
        {
            if (field.Properties.SchemaIds == null || field.Properties.SchemaIds.Count == 0)
            {
                return Enumerable.Empty<IRootField>();
            }

            var allFields =
                field.Properties.SchemaIds
                    .Select(x => field.GetResolvedSchema(x)).NotNull()
                    .SelectMany(x => x.Fields.ForApi(withHidden))
                    .GroupBy(x => new { x.Name, Type = x.RawProperties.GetType() })
                    .Where(x => x.Count() == 1)
                    .Select(x => x.First());

            return allFields;
        }

        public static T SetResolvedSchema<T>(this T metadataProvider, DomainId id, Schema schema) where T : IMetadataProvider
        {
            var keyByName = $"ResolvedSchemaByName_{schema.Name}";
            var keyById = $"ResolvedSchemaById_{id}";

            metadataProvider.Metadata[keyByName] = schema;
            metadataProvider.Metadata[keyById] = schema;

            return metadataProvider;
        }

        public static Schema? GetResolvedSchema<T>(this T metadataProvider, string name) where T : IMetadataProvider
        {
            var key = $"ResolvedSchemaByName_{name}";

            return metadataProvider.GetMetadata<Schema>(key);
        }

        public static Schema? GetResolvedSchema<T>(this T metadataProvider, DomainId id) where T : IMetadataProvider
        {
            var key = $"ResolvedSchemaById_{id}";

            return metadataProvider.GetMetadata<Schema>(key);
        }

        public static bool IsForApi<T>(this T field, bool withHidden = false) where T : IField
        {
            return (withHidden || !field.IsHidden) && !field.RawProperties.IsUIProperty();
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
