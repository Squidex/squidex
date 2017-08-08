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
            Assert.False(sut.IsCompleted);
            Assert.NotEqual(Guid.Empty, sut.ContextId);
        }

        [Fact]
        public void Should_be_handled_when_succeeded()
        {
            var sut = new CommandContext(command);

            sut.Complete();

            Assert.True(sut.IsCompleted);
        }

        [Fact]
        public void Should_provide_result_valid_when_succeeded_with_value()
        {
            var sut = new CommandContext(command);

            sut.Complete("RESULT");
        }
    }
}
