// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed record EventConsumerReset(string EventConsumer)
    {
    }

    public sealed record EventConsumerStart(string EventConsumer)
    {
    }

    public sealed record EventConsumerStop(string EventConsumer)
    {
    }
}
