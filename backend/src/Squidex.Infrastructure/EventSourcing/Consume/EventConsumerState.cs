// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing.Consume
{
    public sealed record EventConsumerState
    {
        public static readonly EventConsumerState Initial = new EventConsumerState();

        public string? Position { get; init; }

        public string? Error { get; init; }

        public bool IsStopped { get; init; }

        public long Count { get; init; }

        public Dictionary<string, string>? Context { get; init; }

        public bool IsPaused
        {
            get => IsStopped && string.IsNullOrWhiteSpace(Error);
        }

        public bool IsFailed
        {
            get => IsStopped && !string.IsNullOrWhiteSpace(Error);
        }

        public EventConsumerState Handled(string position, Dictionary<string, string>? context, int offset = 1)
        {
            return new EventConsumerState
            {
                Context = context,
                Count = Count + offset,
                Position = position
            };
        }

        public EventConsumerState Started()
        {
            return this with { Error = null, IsStopped = false };
        }

        public EventConsumerState Stopped(Exception? ex = null)
        {
            return this with { Error = ex?.Message, IsStopped = true };
        }

        public EventConsumerInfo ToInfo(string name)
        {
            return SimpleMapper.Map(this, new EventConsumerInfo
            {
                Name = name
            });
        }
    }
}
