// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public sealed record EventConsumerInfo
{
    public bool IsStopped { get; init; }

    public int Count { get; init; }

    public string Name { get; init; }

    public string Error { get; init; }

    public string Position { get; init; }
}
