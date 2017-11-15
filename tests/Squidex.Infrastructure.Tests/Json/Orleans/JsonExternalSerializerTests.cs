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
using Newtonsoft.Json;
using Orleans.Serialization;
using Xunit;

namespace Squidex.Infrastructure.Json.Orleans
{
    public class JsonExternalSerializerTests
    {
        private readonly JsonExternalSerializer sut = new JsonExternalSerializer(JsonSerializer.CreateDefault());

        [Fact]
        public void Should_serialize_js_only()
        {
            var serializer = new JsonExternalSerializer(JsonSerializer.CreateDefault(), typeof(int), typeof(bool));

            Assert.True(sut.IsSupportedType(typeof(int)));
            Assert.True(sut.IsSupportedType(typeof(bool)));

            Assert.False(sut.IsSupportedType(typeof(float)));
            Assert.False(sut.IsSupportedType(typeof(double)));
        }

        [Fact]
        public void Should_copy_null()
        {
            var value = (string)null;
            var copy = sut.DeepCopy(value, null);

            Assert.Null(copy);
        }

        [Fact]
        public void Should_copy_non_immutable_values()
        {
            var value = new List<int> { 1, 2, 3 };
            var copy = (List<int>)sut.DeepCopy(value, null);

            Assert.Equal(value, copy);
            Assert.NotSame(value, copy);
        }

        [Fact]
        public void Should_serialize_and_deserialize_value()
        {
            var value = new List<int>(new List<int> { 1, 2, 3 });

            var writtenLength = 0;
            var writtenBuffer = (byte[])null;

            var writer = A.Fake<IBinaryTokenStreamWriter>();
            var writerContext = new SerializationContext(null) { StreamWriter = writer };

            A.CallTo(() => writer.Write(A<int>.Ignored))
                .Invokes(new Action<int>(x => writtenLength = x));

            A.CallTo(() => writer.Write(A<byte[]>.Ignored))
                .Invokes(new Action<byte[]>(x => writtenBuffer = x));

            sut.Serialize(value, writerContext, value.GetType());

            var reader = A.Fake<IBinaryTokenStreamReader>();
            var readerContext = new DeserializationContext(null) { StreamReader = reader };

            A.CallTo(() => reader.ReadInt())
                .Returns(writtenLength);

            A.CallTo(() => reader.ReadBytes(writtenLength))
                .Returns(writtenBuffer);

            var copy = (List<int>)sut.Deserialize(value.GetType(), readerContext);

            Assert.Equal(value, copy);
            Assert.NotSame(value, copy);
        }
    }
}
