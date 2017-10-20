// ==========================================================================
//  SchemaFieldGuard.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public static class SchemaFieldGuard
    {
        public static void GuardCanAdd(Schema schema, string name)
        {
            if (schema.FieldsByName.ContainsKey(name))
            {
                var error = new ValidationError($"There is already a field with name '{name}'", "Name");

                throw new ValidationException("Cannot add a new field.", error);
            }
        }

        public static void GuardCanDelete(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is locked.");
            }
        }

        public static void GuardCanHide(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsHidden)
            {
                throw new DomainException("Schema field is already hidden.");
            }
        }

        public static void GuardCanShow(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (!field.IsHidden)
            {
                throw new DomainException("Schema field is already visible.");
            }
        }

        public static void GuardCanDisable(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsDisabled)
            {
                throw new DomainException("Schema field is already disabled.");
            }
        }

        public static void GuardCanEnable(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (!field.IsDisabled)
            {
                throw new DomainException("Schema field is already enabled.");
            }
        }

        public static void GuardCanLock(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }

        public static void GuardCanUpdate(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }

        private static Field GetFieldOrThrow(Schema schema, long fieldId)
        {
            if (!schema.FieldsById.TryGetValue(fieldId, out var field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Field));
            }

            return field;
        }
    }
}
