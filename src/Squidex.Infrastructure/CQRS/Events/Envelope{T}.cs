// ==========================================================================
//  Envelope_1.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

#pragma warning disable SA1649 // File name must match first type name
namespace Squidex.Infrastructure.CQRS.Events
{
    public class Envelope<TPayload> where TPayload : class
    {
        private readonly EnvelopeHeaders headers;
        private readonly TPayload payload;

        public EnvelopeHeaders Headers
        {
            get { return headers; }
        }

        public TPayload Payload
        {
            get { return payload; }
        }

        public Envelope(TPayload payload)
            : this(payload, new EnvelopeHeaders())
        {
        }

        public Envelope(TPayload payload, PropertiesBag bag)
            : this(payload, new EnvelopeHeaders(bag))
        {
        }

        public Envelope(TPayload payload, EnvelopeHeaders headers)
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
