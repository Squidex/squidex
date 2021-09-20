// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentSchedulerGrainTests
    {
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IClock clock = A.Fake<IClock>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly ContentSchedulerGrain sut;

        public ContentSchedulerGrainTests()
        {
            sut = new ContentSchedulerGrain(contentRepository, commandBus, clock, A.Fake<ISemanticLog>());
        }

        [Fact]
        public async Task Should_change_scheduled_items()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var content1 = new ContentEntity
            {
                AppId = appId,
                Id = DomainId.NewGuid(),
                ScheduleJob = new ScheduleJob(DomainId.NewGuid(), Status.Archived, null!, now)
            };

            var content2 = new ContentEntity
            {
                AppId = appId,
                Id = DomainId.NewGuid(),
                ScheduleJob = new ScheduleJob(DomainId.NewGuid(), Status.Draft, null!, now)
            };

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => contentRepository.QueryScheduledWithoutDataAsync(now, default))
                .Returns(new[] { content1, content2 }.ToAsyncEnumerable());

            await sut.PublishAsync();

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                       x.ContentId == content1.Id &&
                       x.Status == content1.ScheduleJob.Status &&
                       x.StatusJobId == content1.ScheduleJob.Id)))
                .MustHaveHappened();

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                       x.ContentId == content2.Id &&
                       x.Status == content2.ScheduleJob.Status &&
                       x.StatusJobId == content2.ScheduleJob.Id)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_change_status_if_content_has_no_schedule_job()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var content1 = new ContentEntity
            {
                AppId = appId,
                Id = DomainId.NewGuid(),
                ScheduleJob = null,
            };

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => contentRepository.QueryScheduledWithoutDataAsync(now, default))
                .Returns(new[] { content1 }.ToAsyncEnumerable());

            await sut.PublishAsync();

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_reset_job_if_content_not_found_anymore()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var content1 = new ContentEntity
            {
                AppId = appId,
                Id = DomainId.NewGuid(),
                ScheduleJob = new ScheduleJob(DomainId.NewGuid(), Status.Archived, null!, now)
            };

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => contentRepository.QueryScheduledWithoutDataAsync(now, default))
                .Returns(new[] { content1 }.ToAsyncEnumerable());

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .Throws(new DomainObjectNotFoundException(content1.Id.ToString()));

            await sut.PublishAsync();

            A.CallTo(() => contentRepository.ResetScheduledAsync(content1.UniqueId, default))
                .MustHaveHappened();
        }
    }
}
