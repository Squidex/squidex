// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public static class GuardSchema
    {
        public static void CanCreate(CreateSchema command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot create schema.", e =>
            {
                if (!command.Name.IsSlug())
                {
                    e(Not.ValidSlug("Name"), nameof(command.Name));
                }

                ValidateUpsert(command, e);
            });
        }

        public static void CanSynchronize(SynchronizeSchema command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot synchronize schema.", e =>
            {
                ValidateUpsert(command, e);
            });
        }

        public static void CanReorder(ReorderFields command, Schema schema)
        {
            Guard.NotNull(command);

            IArrayField? arrayField = null;

            if (command.ParentFieldId.HasValue)
            {
                arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value, false);
            }

            Validate.It(() => "Cannot reorder schema fields.", e =>
            {
                if (command.FieldIds == null)
                {
                    e(Not.Defined("Field ids"), nameof(command.FieldIds));
                }

                if (arrayField == null)
                {
                    ValidateFieldIds(command, schema.FieldsById, e);
                }
                else
                {
                    ValidateFieldIds(command, arrayField.FieldsById, e);
                }
            });
        }

        public static void CanConfigurePreviewUrls(ConfigurePreviewUrls command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot configure preview urls.", e =>
            {
                if (command.PreviewUrls == null)
                {
                    e(Not.Defined("Preview Urls"), nameof(command.PreviewUrls));
                }
            });
        }

        public static void CanPublish(Schema schema, PublishSchema command)
        {
            Guard.NotNull(command);
        }

        public static void CanUnpublish(Schema schema, UnpublishSchema command)
        {
            Guard.NotNull(command);
        }

        public static void CanConfigureUIFields(ConfigureUIFields command, Schema schema)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot configure UI fields.", e =>
            {
                ValidateFieldNames(schema, command.FieldsInLists, nameof(command.FieldsInLists), e, IsMetaField);
                ValidateFieldNames(schema, command.FieldsInReferences, nameof(command.FieldsInReferences), e, IsNotAllowed);
            });
        }

        public static void CanUpdate(UpdateSchema command)
        {
            Guard.NotNull(command);
        }

        public static void CanConfigureScripts(ConfigureScripts command)
        {
            Guard.NotNull(command);
        }

        public static void CanChangeCategory(ChangeCategory command)
        {
            Guard.NotNull(command);
        }

        public static void CanDelete(DeleteSchema command)
        {
            Guard.NotNull(command);
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

                foreach (var fieldName in command.Fields.Duplicates(x => x.Name))
                {
                    if (fieldName.IsPropertyName())
                    {
                        e($"Field '{fieldName}' has been added twice.", nameof(command.Fields));
                    }
                }
            }

            ValidateFieldNames(command, command.FieldsInLists, nameof(command.FieldsInLists), e, IsMetaField);
            ValidateFieldNames(command, command.FieldsInReferences, nameof(command.FieldsInReferences), e, IsNotAllowed);
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

                    foreach (var fieldName in field.Nested.Duplicates(x => x.Name))
                    {
                        if (fieldName.IsPropertyName())
                        {
                            e($"Field '{fieldName}' has been added twice.", $"{prefix}.Nested");
                        }
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
                e(Not.ValidPropertyName("Field name"), $"{prefix}.{nameof(field.Name)}");
            }

            if (field.Properties == null)
            {
               e(Not.Defined("Field properties"), $"{prefix}.{nameof(field.Properties)}");
            }
            else
            {
                if (field.Properties.IsUIProperty())
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

        private static void ValidateFieldNames(Schema schema, FieldNames? fields, string path, AddValidation e, Func<string, bool> isAllowed)
        {
            if (fields != null)
            {
                var fieldIndex = 0;
                var fieldPrefix = string.Empty;

                foreach (var fieldName in fields)
                {
                    fieldIndex++;
                    fieldPrefix = $"{path}[{fieldIndex}]";

                    var field = schema.FieldsByName.GetOrDefault(fieldName ?? string.Empty);

                    if (string.IsNullOrWhiteSpace(fieldName))
                    {
                        e(Not.Defined("Field"), fieldPrefix);
                    }
                    else if (field == null && !isAllowed(fieldName))
                    {
                        e("Field is not part of the schema.", fieldPrefix);
                    }
                    else if (field?.IsUI() == true)
                    {
                        e("Field cannot be an UI field.", fieldPrefix);
                    }
                }

                foreach (var duplicate in fields.Duplicates())
                {
                    if (!string.IsNullOrWhiteSpace(duplicate))
                    {
                        e($"Field '{duplicate}' has been added twice.", path);
                    }
                }
            }
        }

        private static void ValidateFieldNames(UpsertCommand command, FieldNames? fields, string path, AddValidation e, Func<string, bool> isAllowed)
        {
            if (fields != null)
            {
                var fieldIndex = 0;
                var fieldPrefix = string.Empty;

                foreach (var fieldName in fields)
                {
                    fieldIndex++;
                    fieldPrefix = $"{path}[{fieldIndex}]";

                    var field = command?.Fields?.Find(x => x.Name == fieldName);

                    if (string.IsNullOrWhiteSpace(fieldName))
                    {
                        e(Not.Defined("Field"), fieldPrefix);
                    }
                    else if (field == null && !isAllowed(fieldName))
                    {
                        e("Field is not part of the schema.", fieldPrefix);
                    }
                    else if (field?.Properties?.IsUIProperty() == true)
                    {
                        e("Field cannot be an UI field.", fieldPrefix);
                    }
                }

                foreach (var duplicate in fields.Duplicates())
                {
                    if (!string.IsNullOrWhiteSpace(duplicate))
                    {
                        e($"Field '{duplicate}' has been added twice.", path);
                    }
                }
            }
        }

        private static bool IsMetaField(string field)
        {
            return field.StartsWith("meta.", StringComparison.Ordinal);
        }

        private static bool IsNotAllowed(string field)
        {
            return false;
        }

        private static void ValidateFieldIds<T>(ReorderFields c, IReadOnlyDictionary<long, T> fields, AddValidation e)
        {
            if (c.FieldIds != null && (c.FieldIds.Count != fields.Count || c.FieldIds.Any(x => !fields.ContainsKey(x))))
            {
                e("Field ids do not cover all fields.", nameof(c.FieldIds));
            }
        }
    }
}
