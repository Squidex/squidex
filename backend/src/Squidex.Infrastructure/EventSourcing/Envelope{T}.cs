﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public class Envelope<T> where T : class, IEvent
    {
        private readonly EnvelopeHeaders headers;
        private readonly T payload;

        public EnvelopeHeaders Headers
        {
            get { return headers; }
        }

        public T Payload
        {
            get { return payload; }
        }

        public Envelope(T payload, EnvelopeHeaders? headers = null)
        {
            Guard.NotNull(payload);

            this.payload = payload;

            this.headers = headers ?? new EnvelopeHeaders();
        }

        public Envelope<TOther> To<TOther>() where TOther : class, IEvent
        {
            return new Envelope<TOther>((payload as TOther)!, headers.Clone());
        }

        public static implicit operator Envelope<IEvent>(Envelope<T> source)
        {
            return source == null ? source! : new Envelope<IEvent>(source.payload, source.headers);
        }
    }
}
