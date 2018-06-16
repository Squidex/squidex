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
                    e("Name is not a valid slug.", nameof(command.Name));
                }
                else if (await appProvider.GetSchemaAsync(command.AppId.Id, command.Name) != null)
                {
                    e("A schema with the same name already exists.");
                }

                if (command.Fields?.Count > 0)
                {
                    var fieldIndex = 0;
                    var fieldPrefix = string.Empty;

                    foreach (var field in command.Fields)
                    {
                        fieldIndex++;
                        fieldPrefix = $"Fields[{fieldIndex}]";

                        if (!field.Partitioning.IsValidPartitioning())
                        {
                            e("Field partitioning is not valid.", $"{fieldPrefix}.{nameof(field.Partitioning)}");
                        }

                        ValidateField(e, fieldPrefix, field);

                        if (field.Nested?.Count > 0)
                        {
                            if (field.Properties is ArrayFieldProperties)
                            {
                                var nestedIndex = 0;
                                var nestedPrefix = string.Empty;

                                foreach (var nestedField in field.Nested)
                                {
                                    nestedIndex++;
                                    nestedPrefix = $"{fieldPrefix}.Nested[{nestedIndex}]";

                                    if (nestedField.Properties is ArrayFieldProperties)
                                    {
                                        e("Nested field cannot be array fields.", $"{nestedPrefix}.{nameof(nestedField.Properties)}");
                                    }

                                    ValidateField(e, nestedPrefix, nestedField);
                                }
                            }
                            else if (field.Nested.Count > 0)
                            {
                                e("Only array fields can have nested fields.", $"{fieldPrefix}.{nameof(field.Partitioning)}");
                            }

                            if (field.Nested.Select(x => x.Name).Distinct().Count() != field.Nested.Count)
                            {
                                e("Fields cannot have duplicate names.", $"{fieldPrefix}.Nested");
                            }
                        }
                    }

                    if (command.Fields.Select(x => x.Name).Distinct().Count() != command.Fields.Count)
                    {
                        e("Fields cannot have duplicate names.", nameof(command.Fields));
                    }
                }
            });
        }

        public static void CanReorder(Schema schema, ReorderFields command)
        {
            Guard.NotNull(command, nameof(command));

            IArrayField arrayField = null;

            if (command.ParentFieldId.HasValue)
            {
                arrayField = GuardHelper.GetArrayFieldOrThrow(schema, command.ParentFieldId.Value);
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

        private static void ValidateField(AddValidation e, string prefix, CreateSchemaFieldBase field)
        {
            if (!field.Name.IsPropertyName())
            {
                e("Field name must be a valid javascript property name.", $"{prefix}.{nameof(field.Name)}");
            }

            if (field.Properties == null)
            {
                e("Field properties is required.", $"{prefix}.{nameof(field.Properties)}");
            }
            else
            {
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
