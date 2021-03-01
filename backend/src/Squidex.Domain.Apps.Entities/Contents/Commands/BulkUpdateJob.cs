// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public sealed class BulkUpdateJob
    {
        public Query<IJsonValue>? Query { get; set; }

        public DomainId? Id { get; set; }

        public Status? Status { get; set; }

        public Instant? DueTime { get; set; }

        public BulkUpdateContentType Type { get; set; }

        public ContentData? Data { get; set; }

        public string? Schema { get; set; }

        public bool Permanent { get; set; }

        public long ExpectedCount { get; set; } = 1;

        public long ExpectedVersion { get; set; } = EtagVersion.Any;
    }
}
