// ==========================================================================
//  EventConsumerGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.CQRS.Events.Actors
{
    public sealed class EventConsumerState
    {
        public bool IsStopped { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }

        public EventConsumerState Reset()
        {
            return new EventConsumerState();
        }

        public EventConsumerState Handled(string position)
        {
            return new EventConsumerState { Position = position };
        }

        public EventConsumerState Failed(Exception ex)
        {
            return new EventConsumerState { Position = Position, IsStopped = true, Error = ex?.ToString() };
        }

        public EventConsumerState Stopped()
        {
            return new EventConsumerState { Position = Position, IsStopped = true };
        }

        public EventConsumerState Started()
        {
            return new EventConsumerState { Position = Position, IsStopped = false };
        }

        public EventConsumerInfo ToInfo(string name)
        {
            return SimpleMapper.Map(this, new EventConsumerInfo { Name = name });
        }
    }
}
