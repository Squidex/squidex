// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public class ReadOnlyCollectionTests
    {
        public sealed class MyClass<T>
        {
            public T Values { get; set; }
        }

        [Fact]
        public void Should_serialize_and_deserialize_dictionary_without_type_name()
        {
            var source = new MyClass<IReadOnlyDictionary<int, int>>
            {
                Values = new Dictionary<int, int>
                {
                    [2] = 4,
                    [3] = 9
                }
            };

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver()
            };

            var json = JsonConvert.SerializeObject(source, serializerSettings);

            var serialized = JsonConvert.DeserializeObject<MyClass<IReadOnlyDictionary<int, int>>>(json)!;

            Assert.DoesNotContain("$type", json, StringComparison.Ordinal);
            Assert.Equal(2, serialized.Values.Count);
        }

        [Fact]
        public void Should_serialize_and_deserialize_list_without_type_name()
        {
            var source = new MyClass<IReadOnlyList<int>>
            {
                Values = new List<int>
                {
                    2,
                    3
                }
            };

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver()
            };

            var json = JsonConvert.SerializeObject(source, serializerSettings);

            var serialized = JsonConvert.DeserializeObject<MyClass<IReadOnlyList<int>>>(json)!;

            Assert.DoesNotContain("$type", json, StringComparison.Ordinal);
            Assert.Equal(2, serialized.Values.Count);
        }
    }
}
