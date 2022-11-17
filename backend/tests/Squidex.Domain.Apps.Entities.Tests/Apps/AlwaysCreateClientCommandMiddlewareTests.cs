// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps;

public class AlwaysCreateClientCommandMiddlewareTests
{
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();

    [Fact]
    public async Task Should_create_client()
    {
        var command = new CreateApp { AppId = DomainId.NewGuid(), Name = "my-app" };

        var context =
            new CommandContext(command, commandBus)
                .Complete();

        var sut = new AlwaysCreateClientCommandMiddleware();

        await sut.HandleAsync(context, default);

        A.CallTo(() => commandBus.PublishAsync(A<AttachClient>.That.Matches(x => x.Id == "default"), default))
            .MustHaveHappened();
    }
}
