// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
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
