// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        NamedId<DomainId> AppId { get; }

        NamedId<DomainId> SchemaId { get; }

        Status? NewStatus { get; }

        Status Status { get; }

        ContentData Data { get; }

        ScheduleJob? ScheduleJob { get; }
    }
}
