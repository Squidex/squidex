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
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public static class GuardSchemaField
    {
        public static void CanAdd(AddField command, Schema schema)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot add a new field.", e =>
            {
                if (!command.Name.IsPropertyName())
                {
                    e(Not.ValidPropertyName("Name"), nameof(command.Name));
                }

                if (command.Properties == null)
                {
                   e(Not.Defined("Properties"), nameof(command.Properties));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    errors.Foreach(x => x.WithPrefix(nameof(command.Properties)).AddTo(e));
                }

                if (command.ParentFieldId.HasValue)
                {
                    var arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value, false);

                    if (arrayField.FieldsByName.ContainsKey(command.Name))
                    {
                        e("A field with the same name already exists.");
                    }
                }
                else
                {
                    if (command.ParentFieldId == null && !command.Partitioning.IsValidPartitioning())
                    {
                        e(Not.Valid("Partitioning"), nameof(command.Partitioning));
                    }

                    if (schema.FieldsByName.ContainsKey(command.Name))
                    {
                        e("A field with the same name already exists.");
                    }
                }
            });
        }

        public static void CanUpdate(UpdateField command, Schema schema)
        {
            Guard.NotNull(command);

            GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            Validate.It(() => "Cannot update field.", e =>
            {
                if (command.Properties == null)
                {
                   e(Not.Defined("Properties"), nameof(command.Properties));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    errors.Foreach(x => x.WithPrefix(nameof(command.Properties)).AddTo(e));
                }
            });
        }

        public static void CanHide(HideField command, Schema schema)
        {
            Guard.NotNull(command);

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException("UI field cannot be hidden.");
            }
        }

        public static void CanDisable(DisableField command, Schema schema)
        {
            Guard.NotNull(command);

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException("UI field cannot be diabled.");
            }
        }

        public static void CanShow(ShowField command, Schema schema)
        {
            Guard.NotNull(command);

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException("UI field cannot be shown.");
            }
        }

        public static void CanEnable(EnableField command, Schema schema)
        {
            Guard.NotNull(command);

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException("UI field cannot be enabled.");
            }
        }

        public static void CanDelete(DeleteField command, Schema schema)
        {
            Guard.NotNull(command);

            GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);
        }

        public static void CanLock(LockField command, Schema schema)
        {
            Guard.NotNull(command);

            GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, true);
        }
    }
}
