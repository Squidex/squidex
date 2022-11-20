// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Commands;

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

        Assert.Null(sut.PlainResult);
        Assert.Null(sut.Result<string>());

        Assert.NotEqual(DomainId.Empty, sut.ContextId);

        Assert.False(sut.IsCompleted);
    }

    [Fact]
    public void Should_be_handled_if_succeeded()
    {
        sut.Complete();

        Assert.True(sut.IsCompleted);
    }

    [Fact]
    public void Should_provide_actual_if_succeeded_with_value()
    {
        sut.Complete("RESULT");

        Assert.Equal("RESULT", sut.Result<string>());
    }

    [Fact]
    public void Should_provide_plain_actual_if_succeeded_with_value()
    {
        sut.Complete("RESULT");

        Assert.Equal("RESULT", sut.PlainResult);
    }
}
