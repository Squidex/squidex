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
        /*
        class Context : ISerializationContext
        {
            public IBinaryTokenStreamWriter StreamWriter => throw new NotImplementedException();

            public int CurrentOffset => throw new NotImplementedException();

            public IServiceProvider ServiceProvider => throw new NotImplementedException();

            public object AdditionalContext => throw new NotImplementedException();

            public int CheckObjectWhileSerializing(object raw)
            {
                return 0;
            }

            public void RecordObject(object original, int offset)
            {
            }

            public void SerializeInner(object obj, Type expected)
            {
            }
        }*/

        private readonly JsonExternalSerializer sut = new JsonExternalSerializer(JsonSerializer.CreateDefault());

        public JsonExternalSerializerTests()
        {
        }

        [Fact]
        public void Should_serialize_js_only()
        {
            Assert.True(sut.IsSupportedType(typeof(J<int>)));
            Assert.True(sut.IsSupportedType(typeof(J<List<int>>)));

            Assert.False(sut.IsSupportedType(typeof(int)));
            Assert.False(sut.IsSupportedType(typeof(List<int>)));
        }

        [Fact]
        public void Should_copy_null()
        {
            var value = (string)null;
            var copy = sut.DeepCopy(value, null);

            Assert.Null(copy);
        }

        [Fact]
        public void Should_copy_null_json()
        {
            var value = new J<List<int>>(null);
            var copy = (J<List<int>>)sut.DeepCopy(value, null);

            Assert.Null(copy.Value);
        }

        [Fact]
        public void Should_not_copy_immutable_values()
        {
            var value = new J<List<int>>(new List<int> { 1, 2, 3 }, true);
            var copy = (J<List<int>>)sut.DeepCopy(value, null);

            Assert.Same(value.Value, copy.Value);
        }

        [Fact]
        public void Should_copy_non_immutable_values()
        {
            var value = new J<List<int>>(new List<int> { 1, 2, 3 });
            var copy = (J<List<int>>)sut.DeepCopy(value, null);

            Assert.Equal(value.Value, copy.Value);
            Assert.NotSame(value.Value, copy.Value);
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

            sut.Serialize(value, writerContext, value.GetType());

            var reader = A.Fake<IBinaryTokenStreamReader>();
            var readerContext = new DeserializationContext(null) { StreamReader = reader };

            A.CallTo(() => reader.ReadInt())
                .Returns(writtenLength);

            A.CallTo(() => reader.ReadBytes(writtenLength))
                .Returns(writtenBuffer);

            var copy = (J<List<int>>)sut.Deserialize(value.GetType(), readerContext);

            Assert.Equal(value.Value, copy.Value);
            Assert.NotSame(value.Value, copy.Value);
        }
    }
}
