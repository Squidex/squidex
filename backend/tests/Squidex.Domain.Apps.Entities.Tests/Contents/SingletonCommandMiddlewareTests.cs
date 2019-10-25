﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class SingletonCommandMiddlewareTests
    {
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly SingletonCommandMiddleware sut = new SingletonCommandMiddleware();

        [Fact]
        public async Task Should_create_content_when_singleton_schema_is_created()
        {
            var context =
                new CommandContext(new CreateSchema { IsSingleton = true, Name = "my-schema" }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<CreateContent>.That.Matches(x => x.Publish)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_content_when_non_singleton_schema_is_created()
        {
            var context =
                new CommandContext(new CreateSchema { IsSingleton = false }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_content_when_singleton_schema_not_created()
        {
            var context =
                new CommandContext(new CreateSchema { IsSingleton = true }, commandBus);

            await sut.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .MustNotHaveHappened();
        }
    }
}
