// ==========================================================================
//  JsonExternalSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using Orleans.Serialization;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class JsonExternalSerializerTests
    {
        [Fact]
        public void Should_not_copy_null()
        {
            var v = (string)null;
            var c = J<int>.Copy(v, null);

            Assert.Null(c);
        }

        [Fact]
        public void Should_copy_null_json()
        {
            var v = new J<List<int>>(null);
            var c = (J<List<int>>)J<object>.Copy(v, null);

            Assert.Null(c.Value);
        }

        [Fact]
        public void Should_not_copy_immutable_values()
        {
            var v = new List<int> { 1, 2, 3 }.AsJ();
            var c = (J<List<int>>)J<object>.Copy(v, null);

            Assert.Same(v.Value, c.Value);
        }

        [Fact]
        public void Should_serialize_and_deserialize_value()
        {
            var value = new J<List<int>>(new List<int> { 1, 2, 3 });

            var buffer = new MemoryStream();

            var writer = A.Fake<IBinaryTokenStreamWriter>();
            var writerContext = new SerializationContext(null) { StreamWriter = writer };

            A.CallTo(() => writer.Write(A<byte[]>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .Invokes(new Action<byte[], int, int>(buffer.Write));
            A.CallTo(() => writer.CurrentOffset)
                .ReturnsLazily(x => (int)buffer.Position);

            J<object>.Serialize(value, writerContext, value.GetType());

            buffer.Position = 0;

            var reader = A.Fake<IBinaryTokenStreamReader>();
            var readerContext = new DeserializationContext(null) { StreamReader = reader };

            A.CallTo(() => reader.ReadByteArray(A<byte[]>.Ignored, A<int>.Ignored, A<int>.Ignored))
                .Invokes(new Action<byte[], int, int>((b, o, l) => buffer.Read(b, o, l)));
            A.CallTo(() => reader.CurrentPosition)
                .ReturnsLazily(x => (int)buffer.Position);
            A.CallTo(() => reader.Length)
                .ReturnsLazily(x => (int)buffer.Length);

            var copy = (J<List<int>>)J<object>.Deserialize(value.GetType(), readerContext);

            Assert.Equal(value.Value, copy.Value);
            Assert.NotSame(value.Value, copy.Value);
        }
    }
}
