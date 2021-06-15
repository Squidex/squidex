// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerState
    {
        public static readonly EventConsumerState Initial = new EventConsumerState();

        public bool IsStopped { get; init; }

        public string? Error { get; init; }

        public string? Position { get; init; }

        public int Count { get; init; }

        public bool IsPaused
        {
            get => IsStopped && string.IsNullOrWhiteSpace(Error);
        }

        public bool IsFailed
        {
            get => IsStopped && !string.IsNullOrWhiteSpace(Error);
        }

        public EventConsumerState()
        {
        }

        public EventConsumerState(string? position, int count)
        {
            Position = position;

            Count = count;
        }

        public EventConsumerState Handled(string position, int offset = 1)
        {
            return new EventConsumerState(position, Count + offset);
        }

        public EventConsumerState Stopped(Exception? ex = null)
        {
            return new EventConsumerState(Position, Count) { IsStopped = true, Error = ex?.ToString() };
        }

        public EventConsumerState Started()
        {
            return new EventConsumerState(Position, Count);
        }

        public EventConsumerInfo ToInfo(string name)
        {
            return SimpleMapper.Map(this, new EventConsumerInfo { Name = name });
        }
    }
}
