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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class GuardContent
    {
        public static async Task CanCreate(CreateContent command, IContentWorkflow contentWorkflow, ISchemaEntity schema)
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

        public static async Task CanUpdate(UpdateContent command,
            IContentEntity content,
            IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsUpdate);

            Validate.It(e =>
            {
                ValidateData(command, e);
            });

            await ValidateCanUpdate(content, contentWorkflow, command.User);
        }

        public static async Task CanPatch(PatchContent command,
            IContentEntity content,
            IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsUpdate);

            Validate.It(e =>
            {
                ValidateData(command, e);
            });

            await ValidateCanUpdate(content, contentWorkflow, command.User);
        }

        public static void CanValidate(ValidateContent command, IContentEntity content)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsRead);
        }

        public static void CanDeleteDraft(DeleteContentDraft command, IContentEntity content)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsVersionDelete);

            if (content.NewStatus == null)
            {
                throw new DomainException(T.Get("contents.draftToDeleteNotFound"));
            }
        }

        public static void CanCreateDraft(CreateContentDraft command, IContentEntity content)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsVersionCreate);

            if (content.Status != Status.Published)
            {
                throw new DomainException(T.Get("contents.draftNotCreateForUnpublished"));
            }
        }

        public static Task CanChangeStatus(ChangeContentStatus command,
            IContentEntity content,
            IContentWorkflow contentWorkflow,
            IContentRepository contentRepository,
            ISchemaEntity schema)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsChangeStatus);

            if (schema.SchemaDef.IsSingleton)
            {
                if (content.NewStatus == null || command.Status != Status.Published)
                {
                    throw new DomainException(T.Get("contents.singletonNotChangeable"));
                }

                return Task.CompletedTask;
            }

            return Validate.It(async e =>
            {
                var status = content.NewStatus ?? content.Status;

                if (!await contentWorkflow.CanMoveToAsync(content, status, command.Status, command.User))
                {
                    var values = new { oldStatus = status, newStatus = command.Status };

                    e(T.Get("contents.statusTransitionNotAllowed", values), nameof(command.Status));
                }

                if (content.Status == Status.Published && command.CheckReferrers)
                {
                    var hasReferrer = await contentRepository.HasReferrersAsync(content.AppId.Id, command.ContentId, SearchScope.Published);

                    if (hasReferrer)
                    {
                        throw new DomainException(T.Get("contents.referenced"));
                    }
                }

                if (command.DueTime.HasValue && command.DueTime.Value < SystemClock.Instance.GetCurrentInstant())
                {
                    e(T.Get("contents.statusSchedulingNotInFuture"), nameof(command.DueTime));
                }
            });
        }

        public static async Task CanDelete(DeleteContent command,
            IContentEntity content,
            IContentRepository contentRepository,
            ISchemaEntity schema)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsDeleteOwn);

            if (schema.SchemaDef.IsSingleton)
            {
                throw new DomainException(T.Get("contents.singletonNotDeletable"));
            }

            if (command.CheckReferrers)
            {
                var hasReferrer = await contentRepository.HasReferrersAsync(content.AppId.Id, content.Id, SearchScope.All);

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

        private static async Task ValidateCanUpdate(IContentEntity content, IContentWorkflow contentWorkflow, ClaimsPrincipal? user)
        {
            var status = content.NewStatus ?? content.Status;

            if (!await contentWorkflow.CanUpdateAsync(content, status, user))
            {
                throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status }));
            }
        }

        public static void CheckPermission(IContentEntity content, ContentCommand command, string permission)
        {
            if (Equals(content.CreatedBy, command.Actor) || command.User == null)
            {
                return;
            }

            if (!command.User.Allows(permission, content.AppId.Name, content.SchemaId.Name))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
