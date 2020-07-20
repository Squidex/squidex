// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class ExceptionWrapperFilterTests
    {
        private readonly IIncomingGrainCallContext context = A.Fake<IIncomingGrainCallContext>();
        private readonly ExceptionWrapperFilter sut;

        private sealed class InvalidException : Exception
        {
            public InvalidException(string message)
                : base(message)
            {
            }
        }

        public ExceptionWrapperFilterTests()
        {
            sut = new ExceptionWrapperFilter();
        }

        [Fact]
        public async Task Should_just_forward_serializable_exception()
        {
            var original = new InvalidOperationException();

            A.CallTo(() => context.Invoke())
                .Throws(original);

            var ex = await Assert.ThrowsAnyAsync<Exception>(() => sut.Invoke(context));

            Assert.Same(ex, original);
        }

        [Fact]
        public async Task Should_wrap_non_serializable_exception()
        {
            var original = new InvalidException("My Message");

            A.CallTo(() => context.Invoke())
                .Throws(original);

            var ex = await Assert.ThrowsAnyAsync<OrleansWrapperException>(() => sut.Invoke(context));

            Assert.Equal(original.GetType(), ex.ExceptionType);
            Assert.Contains(original.Message, ex.Message);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var original = new InvalidException("My Message");

            var source = new OrleansWrapperException(original, original.GetType());
            var result = source.SerializeAndDeserializeBinary();

            Assert.Equal(result.ExceptionType, source.ExceptionType);

            Assert.Equal(result.Message, source.Message);
        }
    }
}
