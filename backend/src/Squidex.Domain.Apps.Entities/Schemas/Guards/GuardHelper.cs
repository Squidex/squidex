﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public static class GuardHelper
    {
        public static IArrayField GetArrayFieldOrThrow(Schema schema, long parentId, bool allowLocked)
        {
            if (!schema.FieldsById.TryGetValue(parentId, out var rootField) || !(rootField is IArrayField arrayField))
            {
                throw new DomainObjectNotFoundException(parentId.ToString(), "Fields", typeof(Schema));
            }

            if (!allowLocked)
            {
                EnsureNotLocked(arrayField);
            }

            return arrayField;
        }

        public static IField GetFieldOrThrow(Schema schema, long fieldId, long? parentId, bool allowLocked)
        {
            if (parentId.HasValue)
            {
                var arrayField = GetArrayFieldOrThrow(schema, parentId.Value, allowLocked);

                if (!arrayField.FieldsById.TryGetValue(fieldId, out var nestedField))
                {
                    throw new DomainObjectNotFoundException(fieldId.ToString(), $"Fields[{parentId}].Fields", typeof(Schema));
                }

                return nestedField;
            }

            if (!schema.FieldsById.TryGetValue(fieldId, out var field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Schema));
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
                throw new DomainException("Schema field is locked.");
            }
        }
    }
}
