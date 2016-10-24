// ==========================================================================
//  Envelope.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.CQRS
{
    public class Envelope<TPayload> where TPayload : class
    {
        private readonly EnvelopeHeaders headers;
        private readonly TPayload payload;

        public EnvelopeHeaders Headers
        {
            get
            {
                return headers;
            }
        }

        public TPayload Payload
        {
            get
            {
                return payload;
            }
        }

        public Envelope(TPayload payload, EnvelopeHeaders headers)
        {
            Guard.NotNull(payload, nameof(payload));
            Guard.NotNull(headers, nameof(headers));

            this.payload = payload;
            this.headers = headers;
        }

        public Envelope(TPayload payload)
            : this(payload, new EnvelopeHeaders())
        {
        }

        public Envelope(TPayload payload, PropertiesBag bag)
            : this(payload, new EnvelopeHeaders(bag))
        {
        }

        public Envelope<TOther> To<TOther>() where TOther : class
        {
            return new Envelope<TOther>(payload as TOther, headers.Clone());
        }
    }
}
