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
                if (!command.Partitioning.IsValidPartitioning())
                {
                    error(new ValidationError("Partitioning is not valid.", nameof(command.Partitioning)));
                }

                if (!command.Name.IsPropertyName())
                {
                    error(new ValidationError("Name must be a valid property name.", nameof(command.Name)));
                }

                if (command.Properties == null)
                {
                    error(new ValidationError("Properties is required.", nameof(command.Properties)));
                }

                var propertyErrors = FieldPropertiesValidator.Validate(command.Properties);

                foreach (var propertyError in propertyErrors)
                {
                    error(propertyError);
                }

                if (schema.FieldsByName.ContainsKey(command.Name))
                {
                    error(new ValidationError($"There is already a field with name '{command.Name}'", nameof(command.Name)));
                }
            });
        }

        public static void CanUpdate(Schema schema, UpdateField command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot update field.", error =>
            {
                if (command.Properties == null)
                {
                    error(new ValidationError("Properties is required.", nameof(command.Properties)));
                }

                var propertyErrors = FieldPropertiesValidator.Validate(command.Properties);

                foreach (var propertyError in propertyErrors)
                {
                    error(propertyError);
                }
            });

            var field = GetFieldOrThrow(schema, command.FieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }

        public static void CanDelete(Schema schema, DeleteField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is locked.");
            }
        }

        public static void CanHide(Schema schema, HideField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId);

            if (field.IsHidden)
            {
                throw new DomainException("Schema field is already hidden.");
            }
        }

        public static void CanShow(Schema schema, ShowField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId);

            if (!field.IsHidden)
            {
                throw new DomainException("Schema field is already visible.");
            }
        }

        public static void CanDisable(Schema schema, DisableField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId);

            if (field.IsDisabled)
            {
                throw new DomainException("Schema field is already disabled.");
            }
        }

        public static void CanEnable(Schema schema, EnableField command)
        {
            var field = GetFieldOrThrow(schema, command.FieldId);

            if (!field.IsDisabled)
            {
                throw new DomainException("Schema field is already enabled.");
            }
        }

        public static void CanLock(Schema schema, LockField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GetFieldOrThrow(schema, command.FieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }

        private static Field GetFieldOrThrow(Schema schema, long fieldId)
        {
            if (!schema.FieldsById.TryGetValue(fieldId, out var field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Schema));
            }

            return field;
        }
    }
}
