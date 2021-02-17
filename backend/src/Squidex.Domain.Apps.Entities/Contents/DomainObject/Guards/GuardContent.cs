// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
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
        public static void CanCreate(ContentDataCommand command, ISchemaEntity schema)
        {
            Guard.NotNull(command, nameof(command));

            if (schema.SchemaDef.IsSingleton)
            {
                if (command.ContentId != schema.Id)
                {
                    throw new DomainException(T.Get("contents.singletonNotCreatable"));
                }
            }

            Validate.It(e =>
            {
                if (command.Data == null)
                {
                    e(Not.Defined(nameof(command.Data)), nameof(command.Data));
                }
            });
        }

        public static async Task CanUpdate(ContentDataCommand command, IContentEntity content, IContentWorkflow contentWorkflow)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsUpdate, Permissions.AppContentsUpsert);

            var status = content.NewStatus ?? content.Status;

            if (!await contentWorkflow.CanUpdateAsync(content, status, command.User))
            {
                throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status }));
            }

            Validate.It(e =>
            {
                if (command.Data == null)
                {
                    e(Not.Defined(nameof(command.Data)), nameof(command.Data));
                }
            });
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

        public static async Task CanChangeStatus(ContentCommand command, Status status,
            IContentEntity content,
            IContentWorkflow contentWorkflow,
            IContentRepository contentRepository,
            ISchemaEntity schema)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsChangeStatus, Permissions.AppContentsUpsert);

            if (schema.SchemaDef.IsSingleton)
            {
                if (content.NewStatus == null || status != Status.Published)
                {
                    throw new DomainException(T.Get("contents.singletonNotChangeable"));
                }

                return;
            }

            var oldStatus = content.NewStatus ?? content.Status;

            if (oldStatus == Status.Published && command.CheckReferrers)
            {
                var hasReferrer = await contentRepository.HasReferrersAsync(content.AppId.Id, command.ContentId, SearchScope.Published);

                if (hasReferrer)
                {
                    throw new DomainException(T.Get("contents.referenced"));
                }
            }

            await Validate.It(async e =>
            {
                if (!await contentWorkflow.CanMoveToAsync(content, oldStatus, status, command.User))
                {
                    var values = new { oldStatus, newStatus = status };

                    e(T.Get("contents.statusTransitionNotAllowed", values), "Status");
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

        public static void CanValidate(ValidateContent command, IContentEntity content)
        {
            Guard.NotNull(command, nameof(command));

            CheckPermission(content, command, Permissions.AppContentsRead);
        }

        public static void CheckPermission(IContentEntity content, ContentCommand command, params string[] permissions)
        {
            if (Equals(content.CreatedBy, command.Actor) || command.User == null)
            {
                return;
            }

            if (permissions.All(x => !command.User.Allows(x, content.AppId.Name, content.SchemaId.Name)))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
