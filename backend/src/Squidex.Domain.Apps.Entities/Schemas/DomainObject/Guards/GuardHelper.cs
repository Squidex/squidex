// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards
{
    public static class GuardHelper
    {
        public static IArrayField GetArrayFieldOrThrow(Schema schema, long parentId, bool allowLocked)
        {
            if (!schema.FieldsById.TryGetValue(parentId, out var rootField) || rootField is not IArrayField arrayField)
            {
                throw new DomainObjectNotFoundException(parentId.ToString(CultureInfo.InvariantCulture));
            }

            if (!allowLocked)
            {
                EnsureNotLocked(arrayField);
            }

            return arrayField;
        }

        public static IField GetFieldOrThrow(Schema schema, long fieldId, long? parentId, bool allowLocked)
        {
            if (parentId != null)
            {
                var arrayField = GetArrayFieldOrThrow(schema, parentId.Value, allowLocked);

                if (!arrayField.FieldsById.TryGetValue(fieldId, out var nestedField))
                {
                    throw new DomainObjectNotFoundException(fieldId.ToString(CultureInfo.InvariantCulture));
                }

                return nestedField;
            }

            if (!schema.FieldsById.TryGetValue(fieldId, out var field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(CultureInfo.InvariantCulture));
            }

            if (!allowLocked)
            {
                EnsureNotLocked(field);
            }

            return field;
        }

        private static void EnsureNotLocked(IField field)
        {
            if (field.IsLocked)
            {
                throw new DomainException(T.Get("schemas.fieldIsLocked"));
            }
        }
    }
}
