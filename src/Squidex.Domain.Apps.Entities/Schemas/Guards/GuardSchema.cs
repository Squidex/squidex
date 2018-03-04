// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

                if (command.Fields != null && command.Fields.Any())
                {
                    var index = 0;

                    foreach (var field in command.Fields)
                    {
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

                        var propertyErrors = FieldPropertiesValidator.Validate(field.Properties);

                        foreach (var propertyError in propertyErrors)
                        {
                            error(propertyError);
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

            Validate.It(() => "Cannot reorder schema fields.", error =>
            {
                if (command.FieldIds == null)
                {
                    error(new ValidationError("Field ids is required.", nameof(command.FieldIds)));
                }

                if (command.FieldIds != null && (command.FieldIds.Count != schema.Fields.Count || command.FieldIds.Any(x => !schema.FieldsById.ContainsKey(x))))
                {
                    error(new ValidationError("Ids must cover all fields.", nameof(command.FieldIds)));
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

        public static void CanDelete(Schema schema, DeleteSchema command)
        {
            Guard.NotNull(command, nameof(command));
        }
    }
}
