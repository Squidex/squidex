// ==========================================================================
//  GuardWebhook.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Webhooks.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Webhooks.Guards
{
    public static class GuardWebhook
    {
        public static Task CanCreate(CreateWebhook command, ISchemaProvider schemas)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create webhook.", error => ValidateCommandAsync(command, error, schemas));
        }

        public static Task CanUpdate(UpdateWebhook command, ISchemaProvider schemas)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot update webhook.", error => ValidateCommandAsync(command, error, schemas));
        }

        public static void CanDelete(DeleteWebhook command)
        {
            Guard.NotNull(command, nameof(command));
        }

        private static async Task ValidateCommandAsync(WebhookEditCommand command, Action<ValidationError> error, ISchemaProvider schemas)
        {
            if (command.Url == null || !command.Url.IsAbsoluteUri)
            {
                error(new ValidationError("Url must be specified and absolute.", nameof(command.Url)));
            }

            if (command.Schemas == null)
            {
                error(new ValidationError("Schemas cannot be null.", nameof(command.Schemas)));
            }

            var schemaErrors = await Task.WhenAll(
                command.Schemas.Select(async s =>
                    await schemas.FindSchemaByIdAsync(s.SchemaId) == null
                        ? new ValidationError($"Schema {s.SchemaId} does not exist.", nameof(command.Schemas))
                        : null));

            foreach (var schemaError in schemaErrors.Where(x => x != null))
            {
                error(schemaError);
            }
        }
    }
}
