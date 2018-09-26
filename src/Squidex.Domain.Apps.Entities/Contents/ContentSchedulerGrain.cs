// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Orleans.Runtime;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentSchedulerGrain : Grain, IContentSchedulerGrain, IRemindable
    {
        private readonly Lazy<IContentRepository> contentRepository;
        private readonly Lazy<ICommandBus> commandBus;
        private readonly IClock clock;
        private readonly ISemanticLog log;
        private TaskScheduler scheduler;

        public ContentSchedulerGrain(
            Lazy<IContentRepository> contentRepository,
            Lazy<ICommandBus> commandBus,
            IClock clock,
            ISemanticLog log)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(commandBus, nameof(commandBus));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(log, nameof(log));

            this.contentRepository = contentRepository;
            this.commandBus = commandBus;
            this.clock = clock;
            this.log = log;
        }

        public override Task OnActivateAsync()
        {
            scheduler = TaskScheduler.Current;

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

            return contentRepository.Value.QueryScheduledWithoutDataAsync(now, content =>
            {
                return Dispatch(async () =>
                {
                    try
                    {
                        var job = content.ScheduleJob;

                        if (job != null)
                        {
                            var command = new ChangeContentStatus { ContentId = content.Id, Status = job.Status, Actor = job.ScheduledBy, JobId = job.Id };

                            await commandBus.Value.PublishAsync(command);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, w => w
                            .WriteProperty("action", "ChangeStatusScheduled")
                            .WriteProperty("status", "Failed")
                            .WriteProperty("contentId", content.Id.ToString()));
                    }
                });
            });
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return TaskHelper.Done;
        }

        private Task Dispatch(Func<Task> task)
        {
            return Task<Task>.Factory.StartNew(task, CancellationToken.None, TaskCreationOptions.None, scheduler ?? TaskScheduler.Default).Unwrap();
        }
    }
}
