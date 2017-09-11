// ==========================================================================
//  IHistoryEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.History
{
    public interface IHistoryEventEntity : IEntity
    {
        Guid EventId { get; }

        string Message { get; }

        long Version { get; }

        RefToken Actor { get; }
    }
}
