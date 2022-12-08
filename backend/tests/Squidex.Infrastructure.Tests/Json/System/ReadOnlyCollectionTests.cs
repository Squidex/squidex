// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Json.System;

public class ReadOnlyCollectionTests
{
    public sealed class MyClass<T>
    {
        public T Values { get; set; }
    }

    [Fact]
    public void Should_serialize_and_deserialize_dictionary()
    {
        var source = new MyClass<IReadOnlyDictionary<int, int>>
        {
            Values = new Dictionary<int, int>
            {
                [2] = 4,
                [3] = 9
            }
        };

        var serialized = source.SerializeAndDeserialize();

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

        var serialized = source.SerializeAndDeserialize();

        Assert.Equal(2, serialized.Values.Count);
    }
}
