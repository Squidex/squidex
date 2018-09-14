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

            Validate.It(() => "Cannot add a new field.", e =>
            {
                if (!command.Name.IsPropertyName())
                {
                    e("Name must be a valid javascript property name.", nameof(command.Name));
                }

                if (command.Properties == null)
                {
                    e("Properties is required.", nameof(command.Properties));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    errors.Foreach(x => x.WithPrefix(nameof(command.Properties)).AddTo(e));
                }

                if (command.ParentFieldId.HasValue)
                {
                    var arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value);

                    if (arrayField.FieldsByName.ContainsKey(command.Name))
                    {
                        e("A field with the same name already exists.");
                    }
                }
                else
                {
                    if (command.ParentFieldId == null && !command.Partitioning.IsValidPartitioning())
                    {
                        e("Partitioning is not valid.", nameof(command.Partitioning));
                    }

                    if (schema.FieldsByName.ContainsKey(command.Name))
                    {
                        e("A field with the same name already exists.");
                    }
                }
            });
        }

        public static void CanUpdate(Schema schema, UpdateField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }

            Validate.It(() => "Cannot update field.", e =>
            {
                if (command.Properties == null)
                {
                    e("Properties is required.", nameof(command.Properties));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    errors.Foreach(x => x.WithPrefix(nameof(command.Properties)).AddTo(e));
                }
            });
        }

        public static void CanHide(Schema schema, HideField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

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

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsDisabled)
            {
                throw new DomainException("Schema field is already disabled.");
            }
        }

        public static void CanDelete(Schema schema, DeleteField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is locked.");
            }
        }

        public static void CanShow(Schema schema, ShowField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (!field.IsHidden)
            {
                throw new DomainException("Schema field is already visible.");
            }
        }

        public static void CanEnable(Schema schema, EnableField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (!field.IsDisabled)
            {
                throw new DomainException("Schema field is already enabled.");
            }
        }

        public static void CanLock(Schema schema, LockField command)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }
    }
}
