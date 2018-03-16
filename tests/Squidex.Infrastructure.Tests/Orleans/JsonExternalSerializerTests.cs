// ==========================================================================
//  JsonExternalSerializerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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

            var writtenLength = 0;
            var writtenBuffer = (byte[])null;

            var writer = A.Fake<IBinaryTokenStreamWriter>();
            var writerContext = new SerializationContext(null) { StreamWriter = writer };

            A.CallTo(() => writer.Write(A<int>.Ignored))
                .Invokes(new Action<int>(x => writtenLength = x));

            A.CallTo(() => writer.Write(A<byte[]>.Ignored))
                .Invokes(new Action<byte[]>(x => writtenBuffer = x));

            J<object>.Serialize(value, writerContext, value.GetType());

            var reader = A.Fake<IBinaryTokenStreamReader>();
            var readerContext = new DeserializationContext(null) { StreamReader = reader };

            A.CallTo(() => reader.ReadInt())
                .Returns(writtenLength);

            A.CallTo(() => reader.ReadBytes(writtenLength))
                .Returns(writtenBuffer);

            var copy = (J<List<int>>)J<object>.Deserialize(value.GetType(), readerContext);

            Assert.Equal(value.Value, copy.Value);
            Assert.NotSame(value.Value, copy.Value);
        }
    }
}
