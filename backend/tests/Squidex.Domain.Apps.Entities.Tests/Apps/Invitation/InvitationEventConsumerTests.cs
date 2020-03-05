﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps.Notifications;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation.Notifications
{
    public class InvitationEventConsumerTests
    {
        private readonly INotificationSender notificatíonSender = A.Fake<INotificationSender>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IUser assigner = A.Fake<IUser>();
        private readonly IUser assignee = A.Fake<IUser>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly string assignerId = Guid.NewGuid().ToString();
        private readonly string assigneeId = Guid.NewGuid().ToString();
        private readonly string appName = "my-app";
        private readonly InvitationEventConsumer sut;

        public InvitationEventConsumerTests()
        {
            A.CallTo(() => notificatíonSender.IsActive)
                .Returns(true);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(assigner);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assigneeId))
                .Returns(assignee);

            sut = new InvitationEventConsumer(notificatíonSender, userResolver, log);
        }

        [Fact]
        public async Task Should_not_send_email_if_contributors_assigned_by_clients()
        {
            var @event = CreateEvent(RefTokenType.Client, true);

            await sut.On(@event);

            MustNotResolveUser();
            MustNotSendEmail();
        }

        [Fact]
        public async Task Should_not_send_email_for_initial_owner()
        {
            var @event = CreateEvent(RefTokenType.Subject, false, streamNumber: 1);

            await sut.On(@event);

            MustNotSendEmail();
        }

        [Fact]
        public async Task Should_not_send_email_for_old_events()
        {
            var @event = CreateEvent(RefTokenType.Subject, true, instant: SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(50)));

            await sut.On(@event);

            MustNotResolveUser();
            MustNotSendEmail();
        }

        [Fact]
        public async Task Should_not_send_email_for_old_contributor()
        {
            var @event = CreateEvent(RefTokenType.Subject, true, isNewContributor: false);

            await sut.On(@event);

            MustNotResolveUser();
            MustNotSendEmail();
        }

        [Fact]
        public async Task Should_not_send_email_if_sender_not_active()
        {
            var @event = CreateEvent(RefTokenType.Subject, true);

            A.CallTo(() => notificatíonSender.IsActive)
                .Returns(false);

            await sut.On(@event);

            MustNotResolveUser();
            MustNotSendEmail();
        }

        [Fact]
        public async Task Should_not_send_email_if_assigner_not_found()
        {
            var @event = CreateEvent(RefTokenType.Subject, true);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(Task.FromResult<IUser?>(null));

            await sut.On(@event);

            MustNotSendEmail();
            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_email_if_assignee_not_found()
        {
            var @event = CreateEvent(RefTokenType.Subject, true);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assigneeId))
                .Returns(Task.FromResult<IUser?>(null));

            await sut.On(@event);

            MustNotSendEmail();
            MustLogWarning();
        }

        [Fact]
        public async Task Should_send_email_for_new_user()
        {
            var @event = CreateEvent(RefTokenType.Subject, true);

            await sut.On(@event);

            A.CallTo(() => notificatíonSender.SendInviteAsync(assigner, assignee, appName))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_email_for_existing_user()
        {
            var @event = CreateEvent(RefTokenType.Subject, false);

            await sut.On(@event);

            A.CallTo(() => notificatíonSender.SendInviteAsync(assigner, assignee, appName))
                .MustHaveHappened();
        }

        private void MustLogWarning()
        {
            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }

        private void MustNotResolveUser()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>._))
                .MustNotHaveHappened();
        }

        private void MustNotSendEmail()
        {
            A.CallTo(() => notificatíonSender.SendInviteAsync(A<IUser>._, A<IUser>._, A<string>._))
                .MustNotHaveHappened();
        }

        private Envelope<IEvent> CreateEvent(string assignerType, bool isNewUser, bool isNewContributor = true, Instant? instant = null, int streamNumber = 2)
        {
            var @event = new AppContributorAssigned
            {
                Actor = new RefToken(assignerType, assignerId),
                AppId = NamedId.Of(Guid.NewGuid(), appName),
                ContributorId = assigneeId,
                IsCreated = isNewUser,
                IsAdded = isNewContributor
            };

            var envelope = Envelope.Create(@event);

            envelope.SetTimestamp(instant ?? SystemClock.Instance.GetCurrentInstant());
            envelope.SetEventStreamNumber(streamNumber);

            return envelope;
        }
    }
}
