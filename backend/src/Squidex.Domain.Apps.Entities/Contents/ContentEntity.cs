// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEntity : IEnrichedContentEntity, IContentInfo
    {
        public Guid Id { get; set; }

        public NamedId<Guid> AppId { get; set; }

        public NamedId<Guid> SchemaId { get; set; }

        public long Version { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public NamedContentData Data { get; set; }

        public NamedContentData? ReferenceData { get; set; }

        public ScheduleJob? ScheduleJob { get; set; }

        public Status? NewStatus { get; set; }

        public Status Status { get; set; }

        public StatusInfo[]? NextStatuses { get; set; }

        public bool CanUpdate { get; set; }

        public bool IsSingleton { get; set; }

        public string SchemaName { get; set; }

        public string SchemaDisplayName { get; set; }

        public string StatusColor { get; set; }

        public string? NewStatusColor { get; set; }

        public string? ScheduledStatusColor { get; set; }

        public RootField[]? ReferenceFields { get; set; }

        public NamedContentData EditingData
        {
            get { return Data; }
        }

        public Status EditingStatus
        {
            get { return NewStatus ?? Status; }
        }
    }
}