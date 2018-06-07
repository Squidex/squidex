// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public static class GuardSchemaField
    {
        public static void CanAdd(Schema schema, AddField command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot add a new field.", error =>
            {
                if (!command.Name.IsPropertyName())
                {
                    error(new ValidationError("Name must be a valid property name.", nameof(command.Name)));
                }

                if (command.Properties == null)
                {
                    error(new ValidationError("Properties is required.", nameof(command.Properties)));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    foreach (var e in errors)
                    {
                        error(e.WithPrefix(nameof(command.Properties)));
                    }
                }

                if (command.ParentFieldId.HasValue)
                {
                    var parentId = command.ParentFieldId.Value;

                    if (!schema.FieldsById.TryGetValue(parentId, out var rootField) || !(rootField is IArrayField arrayField))
                    {
                        throw new DomainObjectNotFoundException(parentId.ToString(), "Fields", typeof(Schema));
                    }
                    if (arrayField.FieldsByName.ContainsKey(command.Name))
                    {
                        error(new ValidationError($"There is already a field with name '{command.Name}'", nameof(command.Name)));
                    }
                }
                else
                {
                    if (command.ParentFieldId == null && !command.Partitioning.IsValidPartitioning())
                    {
                        error(new ValidationError("Partitioning is not valid.", nameof(command.Partitioning)));
                    }

                    if (schema.FieldsByName.ContainsKey(command.Name))
                    {
                        error(new ValidationError($"There is already a field with name '{command.Name}'", nameof(command.Name)));
                    }
                }
            });
        }

        public static void CanUpdate(Schema schema, UpdateField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }

            Validate.It(() => "Cannot update field.", error =>
            {
                if (command.Properties == null)
                {
                    error(new ValidationError("Properties is required.", nameof(command.Properties)));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    foreach (var e in errors)
                    {
                        error(e.WithPrefix(nameof(command.Properties)));
                    }
                }
            });
        }

        public static void CanHide(Schema schema, HideField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is locked.");
            }

            if (field.IsHidden)
            {
                throw new DomainException("Schema field is already hidden.");
            }
        }

        public static void CanDisable(Schema schema, DisableField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsDisabled)
            {
                throw new DomainException("Schema field is already disabled.");
            }
        }

        public static void CanDelete(Schema schema, DeleteField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is locked.");
            }
        }

        public static void CanShow(Schema schema, ShowField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (!field.IsHidden)
            {
                throw new DomainException("Schema field is already visible.");
            }
        }

        public static void CanEnable(Schema schema, EnableField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (!field.IsDisabled)
            {
                throw new DomainException("Schema field is already enabled.");
            }
        }

        public static void CanLock(Schema schema, LockField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }

        private static IField GetFieldOrThrow(Schema schema, long fieldId, long? parentId)
        {
            if (parentId.HasValue)
            {
                if (!schema.FieldsById.TryGetValue(parentId.Value, out var rootField) || !(rootField is IArrayField arrayField))
                {
                    throw new DomainObjectNotFoundException(parentId.ToString(), "Fields", typeof(Schema));
                }

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

            return field;
        }
    }
}
