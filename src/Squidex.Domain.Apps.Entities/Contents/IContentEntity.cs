// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentEntity :
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
    {
        NamedId<Guid> AppId { get; }

        NamedId<Guid> SchemaId { get; }

        Status Status { get; }

        ScheduleJob ScheduleJob { get; }

        NamedContentData Data { get; }

        NamedContentData DataDraft { get; }

        bool IsPending { get; }
    }
}
