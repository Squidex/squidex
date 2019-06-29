// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public class ReadOnlyDictionaryTests
    {
        public sealed class MyClass
        {
            public IReadOnlyDictionary<int, int> Values { get; set; }
        }

        [Fact]
        public void Should_serialize_and_deserialize_without_type_name()
        {
            var source = new MyClass
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

            var serialized = JsonConvert.DeserializeObject<MyClass>(json);

            Assert.DoesNotContain("$type", json);
            Assert.Equal(2, serialized.Values.Count);
        }
    }
}
