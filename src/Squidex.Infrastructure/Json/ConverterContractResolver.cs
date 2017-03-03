// ==========================================================================
//  ConverterContractResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Squidex.Infrastructure.Json
{
    public sealed class ConverterContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly JsonConverter[] converters;

        public ConverterContractResolver(params JsonConverter[] converters)
        {
            this.converters = converters;
        }

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            var result = base.ResolveContractConverter(objectType);

            if (result != null)
            {
                return result;
            }

            foreach (var converter in converters)
            {
                if (converter.CanConvert(objectType))
                {
                    return converter;
                }
            }

            return null;
        }
    }
}
