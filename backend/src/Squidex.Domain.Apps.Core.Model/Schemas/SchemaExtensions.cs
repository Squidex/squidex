// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class SchemaExtensions
    {
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
            return schema.Properties.Label.Or(schema.TypeName());
        }

        public static string DisplayNameUnchanged(this Schema schema)
        {
            return schema.Properties.Label.Or(schema.Name);
        }

        public static IEnumerable<RootField> ReferenceFields(this Schema schema)
        {
            return schema.RootFields(schema.FieldsInReferences);
        }

        public static IEnumerable<RootField> ListFields(this Schema schema)
        {
            return schema.RootFields(schema.FieldsInLists);
        }

        public static IEnumerable<RootField> RootFields(this Schema schema, FieldNames names)
        {
            var hasField = false;

            foreach (var name in names)
            {
                if (schema.FieldsByName.TryGetValue(name, out var field))
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
}
