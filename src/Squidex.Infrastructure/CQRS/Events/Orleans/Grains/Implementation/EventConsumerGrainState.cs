// ==========================================================================
//  EventConsumerGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation
{
    public sealed class EventConsumerGrainState
    {
        public bool IsStopped { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }

        public static EventConsumerGrainState Initial()
        {
            return new EventConsumerGrainState();
        }

        public static EventConsumerGrainState Handled(string position)
        {
            return new EventConsumerGrainState { Position = position };
        }

        public static EventConsumerGrainState Failed(Exception ex)
        {
            return new EventConsumerGrainState { IsStopped = true, Error = ex?.ToString() };
        }

        public EventConsumerGrainState Stopped()
        {
            return new EventConsumerGrainState { Position = Position, IsStopped = true };
        }

        public EventConsumerGrainState Started()
        {
            return new EventConsumerGrainState { Position = Position, IsStopped = false };
        }

        public EventConsumerInfo ToInfo(string name)
        {
            return SimpleMapper.Map(this, new EventConsumerInfo { Name = name });
        }
    }
}
