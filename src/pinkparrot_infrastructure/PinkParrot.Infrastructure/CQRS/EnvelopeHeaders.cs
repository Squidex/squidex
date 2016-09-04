// ==========================================================================
//  EnvelopeHeaders.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Infrastructure.CQRS
{
    public sealed class EnvelopeHeaders : PropertiesBag
    {
        public EnvelopeHeaders()
        {
        }

        public EnvelopeHeaders(PropertiesBag bag)
        {
            if (bag == null)
            {
                return;
            }

            foreach (var property in bag.Properties)
            {
                Set(property.Key, property.Value);
            }
        }

        public EnvelopeHeaders Clone()
        {
            var clone = new EnvelopeHeaders();

            foreach (var property in Properties)
            {
                clone.Set(property.Key, property.Value.RawValue);
            }

            return clone;
        }
    }
}
