// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class CommandContextTests
    {
        private readonly ICommand command = new MyCommand();
        private readonly CommandContext sut;

        public CommandContextTests()
        {
            sut = new CommandContext(command, A.Dummy<ICommandBus>());
        }

        [Fact]
        public void Should_instantiate_and_provide_command()
        {
            Assert.Equal(command, sut.Command);
            Assert.False(sut.IsCompleted);
            Assert.NotEqual(Guid.Empty, sut.ContextId);
        }

        [Fact]
        public void Should_be_handled_when_succeeded()
        {
            sut.Complete();

            Assert.True(sut.IsCompleted);
        }

        [Fact]
        public void Should_provide_result_valid_when_succeeded_with_value()
        {
            sut.Complete("RESULT");
        }
    }
}
