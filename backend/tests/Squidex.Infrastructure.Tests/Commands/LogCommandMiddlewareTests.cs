// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Commands;

public class LogCommandMiddlewareTests
{
    private readonly ILogger<LogCommandMiddleware> log = A.Fake<ILogger<LogCommandMiddleware>>();
    private readonly LogCommandMiddleware sut;
    private readonly ICommand command = A.Dummy<ICommand>();
    private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();

    public LogCommandMiddlewareTests()
    {
        A.CallTo(() => log.IsEnabled(A<LogLevel>._))
            .Returns(true);

        sut = new LogCommandMiddleware(log);
    }

    [Fact]
    public async Task Should_log_before_and_after_request()
    {
        var context = new CommandContext(command, commandBus);

        await sut.HandleAsync(context, (c, ct) =>
        {
            context.Complete(true);

            return Task.CompletedTask;
        }, default);

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Debug)
            .MustHaveHappened();

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Information)
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_log_error_if_command_failed()
    {
        var context = new CommandContext(command, commandBus);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await sut.HandleAsync(context, (c, ct) => throw new InvalidOperationException(), default);
        });

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Debug)
            .MustHaveHappened();

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Information)
            .MustHaveHappened();

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Error)
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_log_if_command_is_not_handled()
    {
        var context = new CommandContext(command, commandBus);

        await sut.HandleAsync(context, (c, ct) => Task.CompletedTask, default);

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Debug)
            .MustHaveHappened();

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Information)
            .MustHaveHappenedTwiceExactly();

        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Critical)
            .MustHaveHappened();
    }
}
