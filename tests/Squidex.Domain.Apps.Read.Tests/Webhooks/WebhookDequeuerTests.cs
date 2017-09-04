// ==========================================================================
//  WebhookDequeuerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Infrastructure.Log;
using Xunit;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public class WebhookDequeuerTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly IWebhookRepository webhookRepository = A.Fake<IWebhookRepository>();
        private readonly IWebhookEventRepository webhookEventRepository = A.Fake<IWebhookEventRepository>();
        private readonly WebhookSender webhookSender = A.Fake<WebhookSender>();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();

        public WebhookDequeuerTests()
        {
            A.CallTo(() => clock.GetCurrentInstant()).Returns(now);
        }

        [Fact]
        public void Should_update_repositories_on_successful_requests()
        {
            var @event = CreateEvent(0);

            var requestResult = WebhookResult.Success;
            var requestTime = TimeSpan.FromMinutes(1);
            var requestDump = "Dump";

            SetupSender(@event, requestDump, requestResult, requestTime);
            SetupPendingEvents(@event);

            var sut = new WebhookDequeuer(
                webhookSender,
                webhookEventRepository,
                webhookRepository,
                clock, A.Fake<ISemanticLog>());

            sut.Next();
            sut.Dispose();

            VerifyRepositories(@event, requestDump, requestResult, requestTime, null);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(1, 60)]
        [InlineData(2, 300)]
        [InlineData(3, 360)]
        public void Should_set_next_attempt_based_on_num_calls(int calls, int minutes)
        {
            var @event = CreateEvent(calls);

            var requestResult = WebhookResult.Failed;
            var requestTime = TimeSpan.FromMinutes(1);
            var requestDump = "Dump";

            SetupSender(@event, requestDump, requestResult, requestTime);
            SetupPendingEvents(@event);

            var sut = new WebhookDequeuer(
                webhookSender,
                webhookEventRepository,
                webhookRepository,
                clock, A.Fake<ISemanticLog>());

            sut.Next();
            sut.Dispose();

            VerifyRepositories(@event, requestDump, requestResult, requestTime, now.Plus(Duration.FromMinutes(minutes)));
        }

        private void SetupSender(IWebhookEventEntity @event, string requestDump, WebhookResult requestResult, TimeSpan requestTime)
        {
            A.CallTo(() => webhookSender.SendAsync(@event.Job))
                .Returns((requestDump, requestResult, requestTime));
        }

        private void SetupPendingEvents(IWebhookEventEntity @event)
        {
            A.CallTo(() => webhookEventRepository.QueryPendingAsync(A<Func<IWebhookEventEntity, Task>>.Ignored, A<CancellationToken>.Ignored))
                .Invokes(async (Func<IWebhookEventEntity, Task> callback, CancellationToken ct) =>
                {
                    await callback(@event);
                });
        }

        private void VerifyRepositories(IWebhookEventEntity @event, string requestDump, WebhookResult requestResult, TimeSpan requestTime, Instant? nextAttempt)
        {
            A.CallTo(() => webhookEventRepository.TraceSendingAsync(@event.Id))
                .MustHaveHappened();

            A.CallTo(() => webhookEventRepository.TraceSendingAsync(@event.Id))
                .MustHaveHappened();

            A.CallTo(() => webhookEventRepository.TraceSentAsync(@event.Id, requestDump, requestResult, requestTime, nextAttempt))
                .MustHaveHappened();

            A.CallTo(() => webhookRepository.TraceSentAsync(@event.Job.WebhookId, requestResult, requestTime))
                .MustHaveHappened();
        }

        private static IWebhookEventEntity CreateEvent(int numCalls)
        {
            var @event = A.Fake<IWebhookEventEntity>();

            A.CallTo(() => @event.Id).Returns(Guid.NewGuid());
            A.CallTo(() => @event.Job).Returns(new WebhookJob { WebhookId = Guid.NewGuid() });
            A.CallTo(() => @event.NumCalls).Returns(numCalls);

            return @event;
        }
    }
}
