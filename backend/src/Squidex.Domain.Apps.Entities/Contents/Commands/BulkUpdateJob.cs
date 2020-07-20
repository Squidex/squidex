// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public NamedContentData Data { get; set; }

        public Status? Status { get; set; }

        public BulkUpdateType Type { get; set; }
    }
}
