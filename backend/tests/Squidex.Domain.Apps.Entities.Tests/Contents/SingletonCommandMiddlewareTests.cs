// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents;

public class SingletonCommandMiddlewareTests
{
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly SingletonCommandMiddleware sut = new SingletonCommandMiddleware();

    [Fact]
    public async Task Should_create_content_if_singleton_schema_is_created()
    {
        var command = new CreateSchema { Type = SchemaType.Singleton, Name = "my-schema" };

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await sut.HandleAsync(context, default);

        A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x => x.Status == Status.Published), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_create_content_if_non_singleton_schema_is_created()
    {
        var command = new CreateSchema();

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        await sut.HandleAsync(context, default);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_create_content_if_singleton_schema_not_created()
    {
        var command = new CreateSchema { Type = SchemaType.Singleton };

        var context =
            new CommandContext(command, commandBus);

        await sut.HandleAsync(context, default);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
