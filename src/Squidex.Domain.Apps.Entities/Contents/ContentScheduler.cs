// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentScheduler : IRunnable
    {
        private readonly CompletionTimer timer;
        private readonly IContentRepository contentRepository;
        private readonly ICommandBus commandBus;
        private readonly IClock clock;

        public ContentScheduler(
            IContentRepository contentRepository,
            ICommandBus commandBus,
            IClock clock)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(commandBus, nameof(commandBus));
            Guard.NotNull(clock, nameof(clock));

            this.contentRepository = contentRepository;
            this.commandBus = commandBus;
            this.clock = clock;

            timer = new CompletionTimer(5000, x => PublishAsync());
        }

        public void Run()
        {
        }

        private Task PublishAsync()
        {
            var now = clock.GetCurrentInstant();

            return contentRepository.QueryScheduledWithoutDataAsync(now, content =>
            {
                var command = new ChangeContentStatus { ContentId = content.Id, Status = content.ScheduledTo.Value, Actor = content.ScheduledBy };

                return commandBus.PublishAsync(command);
            });
        }
    }
}
