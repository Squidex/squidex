﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class EnvelopeHeaders : JsonObject
    {
        public EnvelopeHeaders()
        {
        }

        public EnvelopeHeaders(JsonObject headers)
            : base(headers)
        {
        }

        public EnvelopeHeaders Clone()
        {
            return new EnvelopeHeaders(this);
        }
    }
}
