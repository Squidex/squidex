// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.EventSourcing.Consume;

public sealed record EventConsumerState(string? Position, int Count, bool IsStopped = false, string? Error = null)
{
    public static readonly EventConsumerState Initial = new EventConsumerState(null, 0);

    public bool IsPaused
    {
        get => IsStopped && string.IsNullOrWhiteSpace(Error);
    }

    public bool IsFailed
    {
        get => IsStopped && !string.IsNullOrWhiteSpace(Error);
    }

    public EventConsumerState()
        : this(null, 0)
    {
    }

    public EventConsumerState Handled(string? position, int offset = 1)
    {
        return new EventConsumerState(position, Count + offset);
    }

    public EventConsumerState Stopped(Exception? ex = null)
    {
        return new EventConsumerState(Position, Count, true, ex?.Message);
    }

    public EventConsumerState Started()
    {
        return new EventConsumerState(Position, Count);
    }

    public EventConsumerInfo ToInfo(string name)
    {
        return SimpleMapper.Map(this, new EventConsumerInfo
        {
            Name = name
        });
    }
}
