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

        public int Count { get; set; }

        public string? Error { get; set; }

        public string? Position { get; set; }

        public bool IsPaused
        {
            get { return IsStopped && string.IsNullOrWhiteSpace(Error); }
        }

        public bool IsFailed
        {
            get { return IsStopped && !string.IsNullOrWhiteSpace(Error); }
        }

        public EventConsumerState()
        {
        }

        public EventConsumerState(string? position, int count)
        {
            Position = position;

            Count = count;
        }

        public static EventConsumerState Reset()
        {
            return new EventConsumerState();
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
            return new EventConsumerState(Position, Count) { IsStopped = false };
        }

        public EventConsumerInfo ToInfo(string name)
        {
            return SimpleMapper.Map(this, new EventConsumerInfo { Name = name });
        }
    }
}
