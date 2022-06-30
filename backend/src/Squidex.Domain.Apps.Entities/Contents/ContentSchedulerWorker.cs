// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentSchedulerWorker : IInitializable
    {
        private readonly CompletionTimer timer;
        private readonly IContentRepository contentRepository;
        private readonly ICommandBus commandBus;
        private readonly IClock clock;
        private readonly ILogger<ContentSchedulerWorker> log;

        public ContentSchedulerWorker(
            IContentRepository contentRepository,
            ICommandBus commandBus,
            IClock clock,
            ILogger<ContentSchedulerWorker> log)
        {
            this.clock = clock;

            this.commandBus = commandBus;
            this.contentRepository = contentRepository;

            this.log = log;

            timer = new CompletionTimer((int)TimeSpan.FromSeconds(10).TotalMilliseconds, PublishAsync);
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task ReleaseAsync(
            CancellationToken ct)
        {
            return timer.StopAsync();
        }

        public async Task PublishAsync(
            CancellationToken ct = default)
        {
            var now = clock.GetCurrentInstant();

            await foreach (var content in contentRepository.QueryScheduledWithoutDataAsync(now, ct))
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
    }
}
