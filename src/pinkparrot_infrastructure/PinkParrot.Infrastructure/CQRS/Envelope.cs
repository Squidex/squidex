// ==========================================================================
//  Envelope.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Infrastructure.CQRS
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

        public Envelope(TPayload payload)
        {
            Guard.NotNull(payload, nameof(payload));

            this.payload = payload;

            headers = new EnvelopeHeaders();
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
