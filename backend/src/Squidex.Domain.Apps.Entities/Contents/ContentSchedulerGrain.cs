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
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentSchedulerGrain : Grain, IContentSchedulerGrain, IRemindable
    {
        private readonly IContentRepository contentRepository;
        private readonly ICommandBus commandBus;
        private readonly IClock clock;
        private readonly ISemanticLog log;

        public ContentSchedulerGrain(
            IContentRepository contentRepository,
            ICommandBus commandBus,
            IClock clock,
            ISemanticLog log)
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
                log.LogError(ex, content.Id.ToString(), (logContentId, w) => w
                    .WriteProperty("action", "ChangeStatusScheduled")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("contentId", logContentId));
            }
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return Task.CompletedTask;
        }
    }
}
