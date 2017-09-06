// ==========================================================================
//  Envelope_1.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    public class Envelope<T> where T : class
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

        public Envelope(T payload)
            : this(payload, new EnvelopeHeaders())
        {
        }

        public Envelope(T payload, PropertiesBag bag)
            : this(payload, new EnvelopeHeaders(bag))
        {
        }

        public Envelope(T payload, EnvelopeHeaders headers)
        {
            Guard.NotNull(payload, nameof(payload));
            Guard.NotNull(headers, nameof(headers));

            this.payload = payload;
            this.headers = headers;
        }

        public Envelope<TOther> To<TOther>() where TOther : class
        {
            return new Envelope<TOther>(payload as TOther, headers.Clone());
        }
    }
}
