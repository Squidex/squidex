// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

            return Validate.It(() => "Cannot create schema.", async error =>
            {
                if (!command.Name.IsSlug())
                {
                    error(new ValidationError("Name must be a valid slug.", nameof(command.Name)));
                }

                if (await appProvider.GetSchemaAsync(command.AppId.Id, command.Name) != null)
                {
                    error(new ValidationError($"A schema with name '{command.Name}' already exists", nameof(command.Name)));
                }

                if (command.Fields?.Count > 0)
                {
                    var index = 0;

                    foreach (var field in command.Fields)
                    {
                        index++;

                        var prefix = $"Fields.{index}";

                        if (!field.Partitioning.IsValidPartitioning())
                        {
                            error(new ValidationError("Partitioning is not valid.", $"{prefix}.{nameof(field.Partitioning)}"));
                        }

                        if (!field.Name.IsPropertyName())
                        {
                            error(new ValidationError("Name must be a valid property name.", $"{prefix}.{nameof(field.Name)}"));
                        }

                        if (field.Properties == null)
                        {
                            error(new ValidationError("Properties is required.", $"{prefix}.{nameof(field.Properties)}"));
                        }
                        else
                        {
                            var errors = FieldPropertiesValidator.Validate(field.Properties);

                            foreach (var e in errors)
                            {
                                error(e.WithPrefix(prefix));
                            }
                        }

                        if (field.Nested?.Count > 0)
                        {
                            if (!(field.Properties is ArrayFieldProperties))
                            {
                                error(new ValidationError("Only array fields can have nested fields.", $"{prefix}.{nameof(field.Partitioning)}"));
                            }
                            else
                            {
                                var nestedIndex = 0;

                                foreach (var nestedField in field.Nested)
                                {
                                    nestedIndex++;

                                    var nestedPrefix = $"Fields.{index}.Nested.{nestedIndex}";

                                    if (!nestedField.Name.IsPropertyName())
                                    {
                                        error(new ValidationError("Name must be a valid property name.", $"{prefix}.{nameof(nestedField.Name)}"));
                                    }

                                    if (nestedField.Properties == null)
                                    {
                                        error(new ValidationError("Properties is required.", $"{prefix}.{nameof(nestedField.Properties)}"));
                                    }
                                    else
                                    {
                                        var errors = FieldPropertiesValidator.Validate(nestedField.Properties);

                                        foreach (var e in errors)
                                        {
                                            error(e.WithPrefix(nestedPrefix));
                                        }
                                    }
                                }
                            }

                            if (field.Nested.Select(x => x.Name).Distinct().Count() != field.Nested.Count)
                            {
                                error(new ValidationError("Fields cannot have duplicate names.", $"{prefix}.Nested"));
                            }
                        }
                    }

                    if (command.Fields.Select(x => x.Name).Distinct().Count() != command.Fields.Count)
                    {
                        error(new ValidationError("Fields cannot have duplicate names.", nameof(command.Fields)));
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
                var parentId = command.ParentFieldId.Value;

                if (schema.FieldsById.TryGetValue(parentId, out var field) && field is IArrayField a)
                {
                    arrayField = a;
                }
                else
                {
                    throw new DomainObjectNotFoundException(parentId.ToString(), "Fields", typeof(Schema));
                }
            }

            Validate.It(() => "Cannot reorder schema fields.", error =>
            {
                if (command.FieldIds == null)
                {
                    error(new ValidationError("Field ids is required.", nameof(command.FieldIds)));
                }

                if (arrayField == null)
                {
                    CheckFields(error, command, schema.FieldsById);
                }
                else
                {
                    CheckFields(error, command, arrayField.FieldsById);
                }
            });
        }

        private static void CheckFields<T>(Action<ValidationError> error, ReorderFields c, IReadOnlyDictionary<long, T> fields)
        {
            if (c.FieldIds != null && (c.FieldIds.Count != fields.Count || c.FieldIds.Any(x => !fields.ContainsKey(x))))
            {
                error(new ValidationError("Ids must cover all fields.", nameof(c.FieldIds)));
            }
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
    }
}
