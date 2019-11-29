// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class EnvelopeHeadersConverter : JsonValueConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(EnvelopeHeaders); }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var result = base.ReadJson(reader, objectType, existingValue, serializer);

            if (result is JsonObject obj)
            {
                return new EnvelopeHeaders(obj);
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EnvelopeHeaders);
        }
    }
}
