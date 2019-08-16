// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public static class GuardSchema
    {
        public static Task CanCreate(CreateSchema command, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create schema.", async e =>
            {
                if (!command.Name.IsSlug())
                {
                    e(Not.ValidSlug("Name"), nameof(command.Name));
                }
                else if (await appProvider.GetSchemaAsync(command.AppId.Id, command.Name) != null)
                {
                    e("A schema with the same name already exists.");
                }

                ValidateUpsert(command, e);
            });
        }

        public static void CanSynchronize(SynchronizeSchema command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot synchronize schema.", e =>
            {
                ValidateUpsert(command, e);
            });
        }

        public static void CanReorder(Schema schema, ReorderFields command)
        {
            Guard.NotNull(command, nameof(command));

            IArrayField arrayField = null;

            if (command.ParentFieldId.HasValue)
            {
                arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value, false);
            }

            Validate.It(() => "Cannot reorder schema fields.", error =>
            {
                if (command.FieldIds == null)
                {
                    error("Field ids is required.", nameof(command.FieldIds));
                }

                if (arrayField == null)
                {
                    ValidateFieldIds(error, command, schema.FieldsById);
                }
                else
                {
                    ValidateFieldIds(error, command, arrayField.FieldsById);
                }
            });
        }

        public static void CanConfigurePreviewUrls(ConfigurePreviewUrls command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot configure preview urls.", error =>
            {
                if (command.PreviewUrls == null)
                {
                    error("Preview Urls is required.", nameof(command.PreviewUrls));
                }
            });
        }

        public static void CanPublish(Schema schema, PublishSchema command)
        {
            Guard.NotNull(command, nameof(command));

            if (schema.IsPublished)
            {
                throw new DomainException("Schema is already published.");
            }
        }

        public static void CanUnpublish(Schema schema, UnpublishSchema command)
        {
            Guard.NotNull(command, nameof(command));

            if (!schema.IsPublished)
            {
                throw new DomainException("Schema is not published.");
            }
        }

        public static void CanUpdate(Schema schema, UpdateSchema command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanConfigureScripts(Schema schema, ConfigureScripts command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanChangeCategory(Schema schema, ChangeCategory command)
        {
            Guard.NotNull(command, nameof(command));
        }

        public static void CanDelete(Schema schema, DeleteSchema command)
        {
            Guard.NotNull(command, nameof(command));
        }

        private static void ValidateUpsert(UpsertCommand command, AddValidation e)
        {
            if (command.Fields?.Count > 0)
            {
                var fieldIndex = 0;
                var fieldPrefix = string.Empty;

                foreach (var field in command.Fields)
                {
                    fieldIndex++;
                    fieldPrefix = $"Fields[{fieldIndex}]";

                    ValidateRootField(field, fieldPrefix, e);
                }

                if (command.Fields.Select(x => x?.Name).Distinct().Count() != command.Fields.Count)
                {
                    e("Fields cannot have duplicate names.", nameof(command.Fields));
                }
            }
        }

        private static void ValidateRootField(UpsertSchemaField field, string prefix, AddValidation e)
        {
            if (field == null)
            {
                e(Not.Defined("Field"), prefix);
            }
            else
            {
                if (!field.Partitioning.IsValidPartitioning())
                {
                    e(Not.Valid("Partitioning"), $"{prefix}.{nameof(field.Partitioning)}");
                }

                ValidateField(field, prefix, e);

                if (field.Nested?.Count > 0)
                {
                    if (field.Properties is ArrayFieldProperties)
                    {
                        var nestedIndex = 0;
                        var nestedPrefix = string.Empty;

                        foreach (var nestedField in field.Nested)
                        {
                            nestedIndex++;
                            nestedPrefix = $"{prefix}.Nested[{nestedIndex}]";

                            ValidateNestedField(nestedField, nestedPrefix, e);
                        }
                    }
                    else if (field.Nested.Count > 0)
                    {
                        e("Only array fields can have nested fields.", $"{prefix}.{nameof(field.Partitioning)}");
                    }

                    if (field.Nested.Select(x => x.Name).Distinct().Count() != field.Nested.Count)
                    {
                        e("Fields cannot have duplicate names.", $"{prefix}.Nested");
                    }
                }
            }
        }

        private static void ValidateNestedField(UpsertSchemaNestedField nestedField, string prefix, AddValidation e)
        {
            if (nestedField == null)
            {
                e(Not.Defined("Field"), prefix);
            }
            else
            {
                if (nestedField.Properties is ArrayFieldProperties)
                {
                    e("Nested field cannot be array fields.", $"{prefix}.{nameof(nestedField.Properties)}");
                }

                ValidateField(nestedField, prefix, e);
            }
        }

        private static void ValidateField(UpsertSchemaFieldBase field, string prefix, AddValidation e)
        {
            if (!field.Name.IsPropertyName())
            {
                e("Field name must be a valid javascript property name.", $"{prefix}.{nameof(field.Name)}");
            }

            if (field.Properties == null)
            {
               e(Not.Defined("Field properties"), $"{prefix}.{nameof(field.Properties)}");
            }
            else
            {
                if (!field.Properties.IsForApi())
                {
                    if (field.IsHidden)
                    {
                        e("UI field cannot be hidden.", $"{prefix}.{nameof(field.IsHidden)}");
                    }

                    if (field.IsDisabled)
                    {
                        e("UI field cannot be disabled.", $"{prefix}.{nameof(field.IsDisabled)}");
                    }
                }

                var errors = FieldPropertiesValidator.Validate(field.Properties);

                errors.Foreach(x => x.WithPrefix($"{prefix}.{nameof(field.Properties)}").AddTo(e));
            }
        }

        private static void ValidateFieldIds<T>(AddValidation error, ReorderFields c, IReadOnlyDictionary<long, T> fields)
        {
            if (c.FieldIds != null && (c.FieldIds.Count != fields.Count || c.FieldIds.Any(x => !fields.ContainsKey(x))))
            {
                error("Field ids do not cover all fields.", nameof(c.FieldIds));
            }
        }
    }
}
