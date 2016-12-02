// ==========================================================================
//  IHistoryEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.History
{
    public interface IHistoryEventEntity : IEntity
    {
        string Channel { get; }

        string Message { get; }
    }
}
