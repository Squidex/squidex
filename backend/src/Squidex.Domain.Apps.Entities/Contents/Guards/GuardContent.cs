// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Guards
{
    public static class GuardContent
    {
        public static async Task CanCreate(ISchemaEntity schema, IContentWorkflow contentWorkflow, CreateContent command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot created content.", e =>
            {
                ValidateData(command, e);
            });

            if (schema.SchemaDef.IsSingleton && command.ContentId != schema.Id)
            {
                throw new DomainException("Singleton content cannot be created.");
            }

            if (command.Publish && !await contentWorkflow.CanPublishOnCreateAsync(schema, command.Data, command.User))
            {
                throw new DomainException("Content workflow prevents publishing.");
            }
        }

        public static async Task CanUpdate(ContentState content, IContentWorkflow contentWorkflow, UpdateContent command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot update content.", e =>
            {
                ValidateData(command, e);
            });

            await ValidateCanUpdate(content, contentWorkflow, command.User);
        }

        public static async Task CanPatch(ContentState content, IContentWorkflow contentWorkflow, PatchContent command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot patch content.", e =>
            {
                ValidateData(command, e);
            });

            await ValidateCanUpdate(content, contentWorkflow, command.User);
        }

        public static void CanDeleteDraft(DeleteContentDraft command, ISchemaEntity schema, ContentState content)
        {
            Guard.NotNull(command);

            if (schema.SchemaDef.IsSingleton)
            {
                throw new DomainException("Singleton content cannot be updated.");
            }

            if (content.NewStatus == null)
            {
                throw new DomainException("There is nothing to delete.");
            }
        }

        public static void CanCreateDraft(CreateContentDraft command, ISchemaEntity schema, ContentState content)
        {
            Guard.NotNull(command);

            if (schema.SchemaDef.IsSingleton)
            {
                throw new DomainException("Singleton content cannot be updated.");
            }

            if (content.Status != Status.Published)
            {
                throw new DomainException("You can only create a new version when the content is published.");
            }
        }

        public static Task CanChangeStatus(ISchemaEntity schema, ContentState content, IContentWorkflow contentWorkflow, ChangeContentStatus command)
        {
            Guard.NotNull(command);

            if (schema.SchemaDef.IsSingleton)
            {
                throw new DomainException("Singleton content cannot be updated.");
            }

            return Validate.It(() => "Cannot change status.", async e =>
            {
                if (!await contentWorkflow.CanMoveToAsync(content, content.EditingStatus, command.Status, command.User))
                {
                    e($"Cannot change status from {content.Status} to {command.Status}.", nameof(command.Status));
                }

                if (command.DueTime.HasValue && command.DueTime.Value < SystemClock.Instance.GetCurrentInstant())
                {
                    e("Due time must be in the future.", nameof(command.DueTime));
                }
            });
        }

        public static void CanDelete(ISchemaEntity schema, DeleteContent command)
        {
            Guard.NotNull(command);

            if (schema.SchemaDef.IsSingleton)
            {
                throw new DomainException("Singleton content cannot be deleted.");
            }
        }

        private static void ValidateData(ContentDataCommand command, AddValidation e)
        {
            if (command.Data == null)
            {
                e(Not.Defined("Data"), nameof(command.Data));
            }
        }

        private static async Task ValidateCanUpdate(ContentState content, IContentWorkflow contentWorkflow, ClaimsPrincipal user)
        {
            if (!await contentWorkflow.CanUpdateAsync(content, content.EditingStatus, user))
            {
                throw new DomainException($"The workflow does not allow updates at status {content.Status}");
            }
        }
    }
}
