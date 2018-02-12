// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Orleans.Runtime;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentSchedulerGrain : Grain, IContentSchedulerGrain, IRemindable
    {
        private readonly IContentRepository contentRepository;
        private readonly ICommandBus commandBus;
        private readonly IClock clock;

        public ContentSchedulerGrain(
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
        }

        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
            RegisterTimer(x => PublishAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.FromResult(true);
        }

        public Task ActivateAsync()
        {
            return TaskHelper.Done;
        }

        public Task PublishAsync()
        {
            var now = clock.GetCurrentInstant();

            return contentRepository.QueryScheduledWithoutDataAsync(now, content =>
            {
                var command = new ChangeContentStatus { ContentId = content.Id, Status = content.ScheduledTo.Value, Actor = content.ScheduledBy };

                return commandBus.PublishAsync(command);
            });
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return TaskHelper.Done;
        }
    }
}
