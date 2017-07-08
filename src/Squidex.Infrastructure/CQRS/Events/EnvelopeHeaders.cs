// ==========================================================================
//  EnvelopeHeaders.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
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
                Set(property.Key, property.Value.RawValue);
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
