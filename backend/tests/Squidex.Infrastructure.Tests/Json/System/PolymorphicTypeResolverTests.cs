// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json.System;

public class PolymorphicTypeResolverTests
{
    private record Base;

    private record A : Base
    {
        public int PropertyA { get; init; }
    }

    private record B : Base
    {
        public int PropertyB { get; init; }
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var serializer = CreateSerializer();

        Base source = new A
        {
            PropertyA = 42
        };

        var serialized = serializer.Deserialize<Base>(serializer.Serialize(source));

        Assert.Equal(new A { PropertyA = 42 }, serialized);
    }

    [Fact]
    public void Should_deserialize_when_discriminiator_is_first_property()
    {
        var serializer = CreateSerializer();

        var source = new Dictionary<string, object>
        {
            ["$type"] = "A",
            ["propertyA"] = 42,
            ["propertyOther"] = 44
        };

        var serialized = serializer.Deserialize<Base>(serializer.Serialize(source));

        Assert.Equal(new A { PropertyA = 42 }, serialized);
    }

    private static IJsonSerializer CreateSerializer()
    {
        return TestUtils.CreateSerializer(options =>
        {
            options.TypeInfoResolver =
                new PolymorphicTypeResolver(new TypeNameRegistry())
                    .Add<Base>("$type", new Dictionary<string, Type>
                    {
                        ["A"] = typeof(A),
                        ["B"] = typeof(B)
                    });
        });
    }
}
