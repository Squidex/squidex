// ==========================================================================
//  IHistoryEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History
{
    public interface IHistoryEventEntity : IEntity
    {
        Guid EventId { get; }

        RefToken Actor { get; }

        string Message { get; }

        long Version { get; }
    }
}
