// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public class InvitationEmailEventConsumerTests
    {
        private readonly IInvitationEmailSender emailSender = A.Fake<IInvitationEmailSender>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IUser assigner = A.Fake<IUser>();
        private readonly IUser assignee = A.Fake<IUser>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly string assignerId = Guid.NewGuid().ToString();
        private readonly string assigneeId = Guid.NewGuid().ToString();
        private readonly string appName = "my-app";
        private readonly InvitationEmailEventConsumer sut;

        public InvitationEmailEventConsumerTests()
        {
            A.CallTo(() => emailSender.IsActive)
                .Returns(true);

            sut = new InvitationEmailEventConsumer(emailSender, userResolver, log);
        }

        [Fact]
        public async Task Should_ignore_contributors_assigned_by_clients()
        {
            var @event = Envelope.Create(CreateEvent(RefTokenType.Client, true));

            await sut.On(@event);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => emailSender.SendNewUserEmailAsync(A<IUser>.Ignored, A<IUser>.Ignored, appName))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_send_email_if_sender_not_active()
        {
            var @event = Envelope.Create(CreateEvent(RefTokenType.Subject, true));

            A.CallTo(() => emailSender.IsActive)
                .Returns(false);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(Task.FromResult<IUser>(null));

            await sut.On(@event);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>.Ignored))
                .MustNotHaveHappened();

            A.CallTo(() => emailSender.SendNewUserEmailAsync(A<IUser>.Ignored, A<IUser>.Ignored, appName))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_send_email_if_assigner_not_found()
        {
            var @event = Envelope.Create(CreateEvent(RefTokenType.Subject, true));

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(Task.FromResult<IUser>(null));

            await sut.On(@event);

            A.CallTo(() => emailSender.SendNewUserEmailAsync(A<IUser>.Ignored, A<IUser>.Ignored, appName))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_email_if_assignee_not_found()
        {
            var @event = Envelope.Create(CreateEvent(RefTokenType.Subject, true));

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(assigner);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assigneeId))
                .Returns(Task.FromResult<IUser>(null));

            await sut.On(@event);

            A.CallTo(() => emailSender.SendNewUserEmailAsync(A<IUser>.Ignored, A<IUser>.Ignored, appName))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_send_email_for_new_user()
        {
            var @event = Envelope.Create(CreateEvent(RefTokenType.Subject, true));

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(assigner);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assigneeId))
                .Returns(assignee);

            await sut.On(@event);

            A.CallTo(() => emailSender.SendNewUserEmailAsync(assigner, assignee, appName))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_email_for_existing_user()
        {
            var @event = Envelope.Create(CreateEvent(RefTokenType.Subject, false));

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assignerId))
                .Returns(assigner);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(assigneeId))
                .Returns(assignee);

            await sut.On(@event);

            A.CallTo(() => emailSender.SendExistingUserEmailAsync(assigner, assignee, appName))
                .MustHaveHappened();
        }

        private void MustLogWarning()
        {
            A.CallTo(() => log.Log(SemanticLogLevel.Warning, A<None>.Ignored, A<Action<None, IObjectWriter>>.Ignored))
                .MustHaveHappened();
        }

        private IEvent CreateEvent(string assignerType, bool isNew)
        {
            return new AppContributorAssigned
            {
                Actor = new RefToken(assignerType, assignerId),
                AppId = new NamedId<Guid>(Guid.NewGuid(), appName),
                ContributorId = assigneeId,
                IsCreated = isNew
            };
        }
    }
}
