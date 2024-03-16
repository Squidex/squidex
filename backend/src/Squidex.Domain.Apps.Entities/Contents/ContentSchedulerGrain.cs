// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Orleans;
using Orleans.Runtime;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentSchedulerGrain : Grain, IContentSchedulerGrain, IRemindable
    {
        private readonly IContentRepository contentRepository;
        private readonly ICommandBus commandBus;
        private readonly IClock clock;
        private readonly ILogger<ContentSchedulerGrain> log;

        public ContentSchedulerGrain(
            IContentRepository contentRepository,
            ICommandBus commandBus,
            IClock clock,
            ILogger<ContentSchedulerGrain> log)
        {
            this.clock = clock;

            this.commandBus = commandBus;
            this.contentRepository = contentRepository;

            this.log = log;
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
            return Task.CompletedTask;
        }

        public async Task PublishAsync()
        {
            var now = clock.GetCurrentInstant();

            await foreach (var content in contentRepository.QueryScheduledWithoutDataAsync(now))
            {
                await TryPublishAsync(content);
            }
        }

        private async Task TryPublishAsync(IContentEntity content)
        {
            var id = content.Id;

            try
            {
                var job = content.ScheduleJob;

                if (job != null)
                {
                    var command = new ChangeContentStatus
                    {
                        Actor = job.ScheduledBy,
                        AppId = content.AppId,
                        ContentId = id,
                        SchemaId = content.SchemaId,
                        Status = job.Status,
                        StatusJobId = job.Id
                    };

                    await commandBus.PublishAsync(command);
                }
            }
            catch (DomainObjectNotFoundException)
            {
                await contentRepository.ResetScheduledAsync(content.UniqueId, default);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to execute scheduled status change for content '{contentId}'.", content.Id);
            }
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return Task.CompletedTask;
        }
    }
}
