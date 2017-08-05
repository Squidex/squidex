// ==========================================================================
//  CommandContextTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class CommandContextTests
    {
        private readonly ICommand command = A.Dummy<ICommand>();

        [Fact]
        public void Should_instantiate_and_provide_command()
        {
            var sut = new CommandContext(command);

            Assert.Equal(command, sut.Command);
            Assert.Null(sut.Exception);
            Assert.False(sut.IsHandled);
            Assert.False(sut.IsSucceeded);
            Assert.False(sut.IsFailed);
        }

        [Fact]
        public void Should_provide_exception_when_failed()
        {
            var exc = new InvalidOperationException();
            var sut = new CommandContext(command);

            sut.Fail(exc);

            Assert.Equal(exc, sut.Exception);
            Assert.True(sut.IsHandled);
            Assert.True(sut.IsFailed);
            Assert.False(sut.IsSucceeded);
        }

        [Fact]
        public void Should_not_update_exception_when_failed()
        {
            var exc1 = new InvalidOperationException();
            var exc2 = new InvalidOperationException();
            var sut = new CommandContext(command);

            sut.Fail(exc1);
            sut.Fail(exc2);

            Assert.Equal(exc1, sut.Exception);
            Assert.True(sut.IsHandled);
            Assert.True(sut.IsFailed);
            Assert.False(sut.IsSucceeded);
        }

        [Fact]
        public void Should_be_handled_when_succeeded()
        {
            var sut = new CommandContext(command);

            sut.Succeed();

            Assert.Null(sut.Exception);
            Assert.True(sut.IsSucceeded);
            Assert.True(sut.IsHandled);
            Assert.False(sut.IsFailed);
        }

        [Fact]
        public void Should_replace_status_when_already_succeeded()
        {
            var sut = new CommandContext(command);

            sut.Succeed(Guid.NewGuid());
            sut.Fail(new Exception());

            Assert.NotNull(sut.Exception);
            Assert.True(sut.IsHandled);
            Assert.True(sut.IsFailed);
            Assert.True(sut.IsSucceeded);
        }

        [Fact]
        public void Should_provide_result_valid_when_succeeded_with_value()
        {
            var guid = Guid.NewGuid();
            var sut = new CommandContext(command);

            sut.Succeed(guid);

            Assert.Equal(guid, sut.Result<Guid>());
            Assert.True(sut.IsHandled);
            Assert.True(sut.IsSucceeded);
            Assert.False(sut.IsFailed);
        }

        [Fact]
        public void Should_not_change_status_when_already_failed()
        {
            var sut = new CommandContext(command);

            sut.Fail(new Exception());
            sut.Succeed(Guid.NewGuid());

            Assert.NotNull(sut.Exception);
            Assert.True(sut.IsHandled);
            Assert.True(sut.IsFailed);
            Assert.False(sut.IsSucceeded);
        }
    }
}
