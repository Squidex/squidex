// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class ReadonlyCommandMiddlewareTests
    {
        private readonly ICommand command = A.Dummy<ICommand>();
        private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
        private readonly ReadonlyOptions options = new ReadonlyOptions();
        private readonly ReadonlyCommandMiddleware sut;

        public ReadonlyCommandMiddlewareTests()
        {
            sut = new ReadonlyCommandMiddleware(Options.Create(options));
        }

        [Fact]
        public async Task Should_throw_exception_if_readonly()
        {
            var context = new CommandContext(command, commandBus);

            options.IsReadonly = true;

            await Assert.ThrowsAsync<DomainException>(() => MakeCallAsync(context));

            Assert.False(context.IsCompleted);
        }

        [Fact]
        public async Task Should_not_throw_exception_if_not_readonly()
        {
            var context = new CommandContext(command, commandBus);

            options.IsReadonly = false;

            await MakeCallAsync(context);

            Assert.True(context.IsCompleted);
        }

        private async Task MakeCallAsync(CommandContext context)
        {
            await sut.HandleAsync(context, c =>
            {
                context.Complete(true);

                return Task.CompletedTask;
            });
        }
    }
}