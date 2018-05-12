// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEntity : IContentEntity
    {
        public Guid Id { get; set; }

        public NamedId<Guid> AppId { get; set; }

        public NamedId<Guid> SchemaId { get; set; }

        public long Version { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public Status Status { get; set; }

        public ScheduleJob ScheduleJob { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public NamedContentData Data { get; set; }

        public NamedContentData DataDraft { get; set; }

        public bool IsPending { get; set; }

        public static ContentEntity Create(CreateContent command, EntityCreatedResult<NamedContentData> result)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var response = new ContentEntity
            {
                Id = command.ContentId,
                Data = result.IdOrValue,
                Version = result.Version,
                Created = now,
                CreatedBy = command.Actor,
                LastModified = now,
                LastModifiedBy = command.Actor,
                Status = command.Publish ? Status.Published : Status.Draft
            };

            return response;
        }
    }
}