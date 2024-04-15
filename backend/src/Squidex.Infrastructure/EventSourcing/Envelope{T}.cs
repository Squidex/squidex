// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public sealed class Envelope<T>(T payload, EnvelopeHeaders? headers = null) where T : class, IEvent
{
    public EnvelopeHeaders Headers { get; } = headers ?? [];

    public T Payload { get; } = Guard.NotNull(payload);

    public Envelope<TOther> To<TOther>() where TOther : class, IEvent
    {
        return new Envelope<TOther>((payload as TOther)!, Headers.CloneHeaders());
    }

    public static implicit operator Envelope<IEvent>(Envelope<T> source)
    {
        return source == null ? source! : new Envelope<IEvent>(source.Payload, source.Headers);
    }
}
