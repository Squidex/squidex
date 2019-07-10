// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Guards
{
    public static class GuardContent
    {
        public static async Task CanCreate(ISchemaEntity schema, IContentWorkflow contentWorkflow, CreateContent command)
        {
            Guard.NotNull(command, nameof(command));

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

        public static async Task CanUpdate(IContentEntity content, IContentWorkflow contentWorkflow, UpdateContent command, bool isProposal)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot update content.", e =>
            {
                ValidateData(command, e);
            });

            if (!isProposal)
            {
                await ValidateCanUpdate(content, contentWorkflow);
            }
        }

        public static async Task CanPatch(IContentEntity content, IContentWorkflow contentWorkflow, PatchContent command, bool isProposal)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot patch content.", e =>
            {
                ValidateData(command, e);
            });

            if (!isProposal)
            {
                await ValidateCanUpdate(content, contentWorkflow);
            }
        }

        public static void CanDiscardChanges(bool isPending, DiscardChanges command)
        {
            Guard.NotNull(command, nameof(command));

            if (!isPending)
            {
                throw new DomainException("The content has no pending changes.");
            }
        }

        public static Task CanChangeStatus(ISchemaEntity schema, IContentEntity content, IContentWorkflow contentWorkflow, ChangeContentStatus command, bool isChangeConfirm)
        {
            Guard.NotNull(command, nameof(command));

            if (schema.SchemaDef.IsSingleton && command.Status != Status.Published)
            {
                throw new DomainException("Singleton content cannot be changed.");
            }

            return Validate.It(() => "Cannot change status.", async e =>
            {
                if (isChangeConfirm)
                {
                    if (!content.IsPending)
                    {
                        e("Content has no changes to publish.", nameof(command.Status));
                    }
                }
                else if (!await contentWorkflow.CanMoveToAsync(content, command.Status, command.User))
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
            Guard.NotNull(command, nameof(command));

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

        private static async Task ValidateCanUpdate(IContentEntity content, IContentWorkflow contentWorkflow)
        {
            if (!await contentWorkflow.CanUpdateAsync(content))
            {
                throw new DomainException($"The workflow does not allow updates at status {content.Status}");
            }
        }
    }
}
