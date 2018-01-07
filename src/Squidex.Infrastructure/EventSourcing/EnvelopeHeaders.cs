// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
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
