// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing.Grains
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
