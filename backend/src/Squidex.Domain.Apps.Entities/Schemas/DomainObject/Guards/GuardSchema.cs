// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject.Guards
{
    public static class GuardSchema
    {
        public static void CanCreate(CreateSchema command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (!command.Name.IsSlug())
                {
                    e(Not.ValidSlug(nameof(command.Name)), nameof(command.Name));
                }

                ValidateUpsert(command, e);
            });
        }

        public static void CanSynchronize(SynchronizeSchema command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                ValidateUpsert(command, e);
            });
        }

        public static void CanReorder(ReorderFields command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            IArrayField? arrayField = null;

            if (command.ParentFieldId != null)
            {
                arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value, false);
            }

            Validate.It(e =>
            {
                if (command.FieldIds == null)
                {
                    e(Not.Defined(nameof(command.FieldIds)), nameof(command.FieldIds));
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
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (command.PreviewUrls == null)
                {
                    e(Not.Defined(nameof(command.PreviewUrls)), nameof(command.PreviewUrls));
                }
            });
        }

        public static void CanConfigureUIFields(ConfigureUIFields command, Schema schema)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                ValidateFieldNames(schema, command.FieldsInLists, nameof(command.FieldsInLists), e, IsMetaField);
                ValidateFieldNames(schema, command.FieldsInReferences, nameof(command.FieldsInReferences), e, IsNotAllowed);
            });
        }

        public static void CanConfigureFieldRules(ConfigureFieldRules command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                ValidateFieldRules(command.FieldRules, nameof(command.FieldRules), e);
            });
        }

        private static void ValidateUpsert(IUpsertCommand command, AddValidation e)
        {
            if (command.Fields?.Length > 0)
            {
                command.Fields.Foreach((field, fieldIndex) =>
                {
                    var fieldPrefix = $"Fields[{fieldIndex}]";

                    ValidateRootField(field, fieldPrefix, e);
                });

                foreach (var fieldName in command.Fields.Duplicates(x => x.Name))
                {
                    if (fieldName.IsPropertyName())
                    {
                        e(T.Get("schemas.duplicateFieldName", new { field = fieldName }), nameof(command.Fields));
                    }
                }
            }

            ValidateFieldNames(command, command.FieldsInLists, nameof(command.FieldsInLists), e, IsMetaField);
            ValidateFieldNames(command, command.FieldsInReferences, nameof(command.FieldsInReferences), e, IsNotAllowed);

            ValidateFieldRules(command.FieldRules, nameof(command.FieldRules), e);
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
                    e(Not.Valid(nameof(field.Partitioning)), $"{prefix}.{nameof(field.Partitioning)}");
                }

                ValidateField(field, prefix, e);

                if (field.Nested?.Length > 0)
                {
                    if (field.Properties is ArrayFieldProperties)
                    {
                        field.Nested.Foreach((nestedField, nestedIndex) =>
                        {
                            var nestedPrefix = $"{prefix}.Nested[{nestedIndex}]";

                            ValidateNestedField(nestedField, nestedPrefix, e);
                        });
                    }
                    else if (field.Nested.Length > 0)
                    {
                        e(T.Get("schemas.onlyArraysHaveNested"), $"{prefix}.{nameof(field.Partitioning)}");
                    }

                    foreach (var fieldName in field.Nested.Duplicates(x => x.Name))
                    {
                        if (fieldName.IsPropertyName())
                        {
                            e(T.Get("schemas.duplicateFieldName", new { field = fieldName }), $"{prefix}.Nested");
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
                    e(T.Get("schemas.onylArraysInRoot"), $"{prefix}.{nameof(nestedField.Properties)}");
                }

                ValidateField(nestedField, prefix, e);
            }
        }

        private static void ValidateField(UpsertSchemaFieldBase field, string prefix, AddValidation e)
        {
            if (!field.Name.IsPropertyName())
            {
                e(Not.ValidJavascriptName(nameof(field.Name)), $"{prefix}.{nameof(field.Name)}");
            }

            if (field.Properties == null)
            {
               e(Not.Defined(nameof(field.Properties)), $"{prefix}.{nameof(field.Properties)}");
            }
            else
            {
                if (field.Properties.IsUIProperty())
                {
                    if (field.IsHidden)
                    {
                        e(T.Get("schemas.uiFieldCannotBeHidden"), $"{prefix}.{nameof(field.IsHidden)}");
                    }

                    if (field.IsDisabled)
                    {
                        e(T.Get("schemas.uiFieldCannotBeDisabled"), $"{prefix}.{nameof(field.IsDisabled)}");
                    }
                }

                var errors = FieldPropertiesValidator.Validate(field.Properties);

                errors.Foreach((x, _) => x.WithPrefix($"{prefix}.{nameof(field.Properties)}").AddTo(e));
            }
        }

        private static void ValidateFieldNames(Schema schema, FieldNames? fields, string path, AddValidation e, Func<string, bool> isAllowed)
        {
            if (fields != null)
            {
                fields.Foreach((fieldName, fieldIndex) =>
                {
                    var fieldPrefix = $"{path}[{fieldIndex}]";

                    var field = schema.FieldsByName.GetOrDefault(fieldName ?? string.Empty);

                    if (string.IsNullOrWhiteSpace(fieldName))
                    {
                        e(Not.Defined("Field"), fieldPrefix);
                    }
                    else if (field == null && !isAllowed(fieldName))
                    {
                        e(T.Get("schemas.fieldNotInSchema"), fieldPrefix);
                    }
                    else if (field?.IsUI() == true)
                    {
                        e(T.Get("schemas.fieldCannotBeUIField"), fieldPrefix);
                    }
                });

                foreach (var duplicate in fields.Duplicates())
                {
                    if (!string.IsNullOrWhiteSpace(duplicate))
                    {
                        e(T.Get("schemas.duplicateFieldName", new { field = duplicate }), path);
                    }
                }
            }
        }

        private static void ValidateFieldRules(FieldRuleCommand[]? fieldRules, string path, AddValidation e)
        {
            fieldRules?.Foreach((rule, ruleIndex) =>
            {
                var rulePrefix = $"{path}[{ruleIndex}]";

                if (string.IsNullOrWhiteSpace(rule.Field))
                {
                    e(Not.Defined(nameof(rule.Field)), $"{rulePrefix}.{nameof(rule.Field)}");
                }

                if (!rule.Action.IsEnumValue())
                {
                    e(Not.Valid(nameof(rule.Action)), $"{rulePrefix}.{nameof(rule.Action)}");
                }
            });
        }

        private static void ValidateFieldNames(IUpsertCommand command, FieldNames? fields, string path, AddValidation e, Func<string, bool> isAllowed)
        {
            if (fields != null)
            {
                fields.Foreach((fieldName, fieldIndex) =>
                {
                    var fieldPrefix = $"{path}[{fieldIndex}]";

                    var field = command?.Fields?.FirstOrDefault(x => x.Name == fieldName);

                    if (string.IsNullOrWhiteSpace(fieldName))
                    {
                        e(Not.Defined("Field"), fieldPrefix);
                    }
                    else if (field == null && !isAllowed(fieldName))
                    {
                        e(T.Get("schemas.fieldNotInSchema"), fieldPrefix);
                    }
                    else if (field?.Properties?.IsUIProperty() == true)
                    {
                        e(T.Get("schemas.fieldCannotBeUIField"), fieldPrefix);
                    }
                });

                foreach (var duplicate in fields.Duplicates())
                {
                    if (!string.IsNullOrWhiteSpace(duplicate))
                    {
                        e(T.Get("schemas.duplicateFieldName", new { field = duplicate }), path);
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

        private static void ValidateFieldIds<TField>(ReorderFields c, IReadOnlyDictionary<long, TField> fields, AddValidation e)
        {
            if (c.FieldIds != null && (c.FieldIds.Length != fields.Count || c.FieldIds.Any(x => !fields.ContainsKey(x))))
            {
                e(T.Get("schemas.fieldsNotCovered"), nameof(c.FieldIds));
            }
        }
    }
}
