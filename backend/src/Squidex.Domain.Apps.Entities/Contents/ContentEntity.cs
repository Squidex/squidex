// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEntity : IEnrichedContentEntity
    {
        public Guid Id { get; set; }

        public NamedId<Guid> AppId { get; set; }

        public NamedId<Guid> SchemaId { get; set; }

        public long Version { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public ScheduleJob ScheduleJob { get; set; }

        public NamedContentData? Data { get; set; }

        public NamedContentData DataDraft { get; set; }

        public NamedContentData? ReferenceData { get; set; }

        public Status Status { get; set; }

        public StatusInfo[]? Nexts { get; set; }

        public string StatusColor { get; set; }

        public string SchemaName { get; set; }

        public string SchemaDisplayName { get; set; }

        public RootField[]? ReferenceFields { get; set; }

        public bool CanUpdate { get; set; }

        public bool IsPending { get; set; }

        public HashSet<object?> CacheDependencies { get; set; }
    }
}