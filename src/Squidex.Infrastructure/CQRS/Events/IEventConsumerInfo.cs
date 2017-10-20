// ==========================================================================
//  IEventConsumerInfo.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventConsumerInfo
    {
        bool IsStopped { get; }

        string Name { get; }

        string Error { get; }

        string Position { get; }
    }
}