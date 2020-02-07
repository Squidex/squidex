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

        Status? NewStatus { get; }

        Status Status { get; }

        NamedContentData Data { get; }

        ScheduleJob? ScheduleJob { get; }
    }
}
