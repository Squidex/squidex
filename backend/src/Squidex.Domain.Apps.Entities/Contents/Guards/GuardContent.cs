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
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.Guards
{
    public static class GuardContent
    {
        public static async Task CanCreate(ISchemaEntity schema, IContentWorkflow contentWorkflow, CreateContent command)
        {
            Guard.NotNull(command, nameof(command));

            if (schema.SchemaDef.IsSingleton && command.ContentId != schema.Id)
            {
                throw new DomainException(T.Get("contents.singletonNotCreatable"));
            }

            if (command.Publish && !await contentWorkflow.CanPublishOnCreateAsync(schema, command.Data, command.User))
            {
                throw new DomainException(T.Get("contents.workflowErorPublishing"));
            }

            Validate.It(e =>
            {
                ValidateData(command, e);
            });
        }

        public static async Task CanUpdate(ContentState content, IContentWorkflow contentWorkflow, UpdateContent command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                ValidateData(command, e);
            });

            await ValidateCanUpdate(content, contentWorkflow, command.User);
        }

        public static async Task CanPatch(ContentState content, IContentWorkflow contentWorkflow, PatchContent command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                ValidateData(command, e);
            });

            await ValidateCanUpdate(content, contentWorkflow, command.User);
        }

        public static void CanDeleteDraft(DeleteContentDraft command, ContentState content)
        {
            Guard.NotNull(command, nameof(command));

            if (content.NewStatus == null)
            {
                throw new DomainException(T.Get("contents.draftToDeleteNotFound"));
            }
        }

        public static void CanCreateDraft(CreateContentDraft command, ContentState content)
        {
            Guard.NotNull(command, nameof(command));

            if (content.Status != Status.Published)
            {
                throw new DomainException(T.Get("contents.draftNotCreateForUnpublished"));
            }
        }

        public static Task CanChangeStatus(ISchemaEntity schema, ContentState content, IContentWorkflow contentWorkflow, ChangeContentStatus command)
        {
            Guard.NotNull(command, nameof(command));

            if (schema.SchemaDef.IsSingleton)
            {
                if (content.NewVersion == null || command.Status != Status.Published)
                {
                    throw new DomainException(T.Get("contents.singletonNotChangeable"));
                }

                return Task.CompletedTask;
            }

            return Validate.It(async e =>
            {
                if (!await contentWorkflow.CanMoveToAsync(content, content.EditingStatus, command.Status, command.User))
                {
                    e(T.Get("contents.statusTransitionNotAllowed", new { oldStatus = content.EditingStatus, newStatus = command.Status }), nameof(command.Status));
                }

                if (command.DueTime.HasValue && command.DueTime.Value < SystemClock.Instance.GetCurrentInstant())
                {
                    e(T.Get("contents.statusSchedulingNotInFuture"), nameof(command.DueTime));
                }
            });
        }

        public static async Task CanDelete(ISchemaEntity schema, ContentState content, IContentRepository contentRepository, DeleteContent command)
        {
            Guard.NotNull(command, nameof(command));

            if (schema.SchemaDef.IsSingleton)
            {
                throw new DomainException(T.Get("contents.singletonNotDeletable"));
            }

            if (command.CheckReferrers)
            {
                var hasReferrer = await contentRepository.HasReferrersAsync(content.AppId.Id, command.ContentId);

                if (hasReferrer)
                {
                    throw new DomainException(T.Get("contents.referenced"));
                }
            }
        }

        private static void ValidateData(ContentDataCommand command, AddValidation e)
        {
            if (command.Data == null)
            {
                e(Not.Defined(nameof(command.Data)), nameof(command.Data));
            }
        }

        private static async Task ValidateCanUpdate(ContentState content, IContentWorkflow contentWorkflow, ClaimsPrincipal user)
        {
            if (!await contentWorkflow.CanUpdateAsync(content, content.EditingStatus, user))
            {
                throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status = content.EditingStatus }));
            }
        }
    }
}
