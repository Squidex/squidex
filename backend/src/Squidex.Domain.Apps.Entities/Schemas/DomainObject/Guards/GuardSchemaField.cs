// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards
{
    public static class GuardSchemaField
    {
        public static void CanAdd(AddField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (!command.Name.IsPropertyName())
                {
                    e(Not.ValidJavascriptName(nameof(command.Name)), nameof(command.Name));
                }

                if (command.Properties == null)
                {
                    e(Not.Defined(nameof(command.Properties)), nameof(command.Properties));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    errors.Foreach((x, _) => x.WithPrefix(nameof(command.Properties)).AddTo(e));
                }

                if (command.ParentFieldId != null)
                {
                    var arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value, false);

                    if (arrayField.FieldsByName.ContainsKey(command.Name))
                    {
                        e(T.Get("schemas.fieldNameAlreadyExists"));
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
                        e(T.Get("schemas.fieldNameAlreadyExists"));
                    }
                }
            });
        }

        public static void CanUpdate(UpdateField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            Validate.It(e =>
            {
                if (command.Properties == null)
                {
                    e(Not.Defined("Properties"), nameof(command.Properties));
                }
                else
                {
                    var errors = FieldPropertiesValidator.Validate(command.Properties);

                    errors.Foreach((x, _) => x.WithPrefix(nameof(command.Properties)).AddTo(e));
                }
            });
        }

        public static void CanHide(HideField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException(T.Get("schemas.uiFieldCannotBeHidden"));
            }
        }

        public static void CanDisable(DisableField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException(T.Get("schemas.uiFieldCannotBeDisabled"));
            }
        }

        public static void CanShow(ShowField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException(T.Get("schemas.uiFieldCannotBeShown"));
            }
        }

        public static void CanEnable(EnableField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            var field = GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);

            if (!field.IsForApi(true))
            {
                throw new DomainException(T.Get("schemas.uiFieldCannotBeEnabled"));
            }
        }

        public static void CanDelete(DeleteField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, false);
        }

        public static void CanLock(LockField command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            GuardHelper.GetFieldOrThrow(schema, command.FieldId, command.ParentFieldId, true);
        }
    }
}
