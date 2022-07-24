// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json.System
{
    public class JsonInheritanceConverterBaseTests
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

        private class Converter : InheritanceConverterBase<Base>
        {
            public Converter()
                : base("$type")
            {
            }

            public override Type GetDiscriminatorType(string name, Type typeToConvert)
            {
                return name == "A" ? typeof(A) : typeof(B);
            }

            public override string GetDiscriminatorValue(Type type)
            {
                return type == typeof(A) ? "A" : "B";
            }
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

        [Fact]
        public void Should_deserialize_when_discriminiator_is_not_first_property()
        {
            var serializer = CreateSerializer();

            var source = new Dictionary<string, object>
            {
                ["propertyB"] = 42,
                ["propertyOther"] = 44,
                ["$type"] = "B"
            };

            var serialized = serializer.Deserialize<Base>(serializer.Serialize(source));

            Assert.Equal(new B { PropertyB = 42 }, serialized);
        }

        private static IJsonSerializer CreateSerializer()
        {
            return TestUtils.CreateSerializer(options =>
            {
                options.Converters.Add(new Converter());
            });
        }
    }
}
