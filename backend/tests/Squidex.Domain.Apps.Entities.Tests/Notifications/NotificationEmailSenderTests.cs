// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure.Email;
using Squidex.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Notifications
{
    public class NotificationEmailSenderTests
    {
        private readonly IEmailSender emailSender = A.Fake<IEmailSender>();
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly IUser assigner = UserMocks.User("1", "1@email.com", "user1");
        private readonly IUser assigned = UserMocks.User("2", "2@email.com", "user2");
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly string appName = "my-app";
        private readonly string appUI = "my-ui";
        private readonly NotificationEmailTextOptions texts = new NotificationEmailTextOptions();
        private readonly NotificationEmailSender sut;

        public NotificationEmailSenderTests()
        {
            A.CallTo(() => urlGenerator.UI())
                .Returns(appUI);

            sut = new NotificationEmailSender(Options.Create(texts), emailSender, urlGenerator, log);
        }

        [Fact]
        public async Task Should_format_assigner_email_and_send_email()
        {
            await TestInvitationFormattingAsync("Email: $ASSIGNER_EMAIL", "Email: 1@email.com");
        }

        [Fact]
        public async Task Should_format_assigner_name_and_send_email()
        {
            await TestInvitationFormattingAsync("Name: $ASSIGNER_NAME", "Name: user1");
        }

        [Fact]
        public async Task Should_format_user_email_and_send_email()
        {
            await TestInvitationFormattingAsync("Email: $USER_EMAIL", "Email: 2@email.com");
        }

        [Fact]
        public async Task Should_format_user_name_and_send_email()
        {
            await TestInvitationFormattingAsync("Name: $USER_NAME", "Name: user2");
        }

        [Fact]
        public async Task Should_format_app_name_and_send_email()
        {
            await TestInvitationFormattingAsync("App: $APP_NAME", "App: my-app");
        }

        [Fact]
        public async Task Should_format_ui_url_and_send_email()
        {
            await TestInvitationFormattingAsync("UI: $UI_URL", "UI: my-ui");
        }

        [Fact]
        public async Task Should_format_api_calls_and_send_email()
        {
            await TestUsageFormattingAsync("ApiCalls: $API_CALLS", "ApiCalls: 100");
        }

        [Fact]
        public async Task Should_format_api_calls_limit_and_send_email()
        {
            await TestUsageFormattingAsync("ApiCallsLimit: $API_CALLS_LIMIT", "ApiCallsLimit: 120");
        }

        [Fact]
        public async Task Should_not_send_invitation_email_if_texts_for_new_user_are_empty()
        {
            await sut.SendInviteAsync(assigner, assigned, appName);

            A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_invitation_email_if_texts_for_existing_user_are_empty()
        {
            await sut.SendInviteAsync(assigner, assigned, appName);

            A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_usage_email_if_texts_empty()
        {
            await sut.SendUsageAsync(assigned, appName, 100, 120);

            A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_invitation_email_if_no_consent_given()
        {
            var withoutConsent = UserMocks.User("2", "2@email.com", "user", false);

            texts.ExistingUserSubject = "email-subject";
            texts.ExistingUserBody = "email-body";

            await sut.SendInviteAsync(assigner, withoutConsent, appName);

            A.CallTo(() => emailSender.SendAsync(withoutConsent.Email, "email-subject", "email-body", A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_send_invitation_email_if_consent_given()
        {
            var withConsent = UserMocks.User("2", "2@email.com", "user", true);

            texts.ExistingUserSubject = "email-subject";
            texts.ExistingUserBody = "email-body";

            await sut.SendInviteAsync(assigner, withConsent, appName);

            A.CallTo(() => emailSender.SendAsync(withConsent.Email, "email-subject", "email-body", A<CancellationToken>._))
                .MustHaveHappened();
        }

        private async Task TestUsageFormattingAsync(string pattern, string result)
        {
            texts.UsageSubject = pattern;
            texts.UsageBody = pattern;

            await sut.SendUsageAsync(assigned, appName, 100, 120);

            A.CallTo(() => emailSender.SendAsync(assigned.Email, result, result, A<CancellationToken>._))
                .MustHaveHappened();
        }

        private async Task TestInvitationFormattingAsync(string pattern, string result)
        {
            texts.NewUserSubject = pattern;
            texts.NewUserBody = pattern;

            await sut.SendInviteAsync(assigner, assigned, appName);

            A.CallTo(() => emailSender.SendAsync(assigned.Email, result, result, A<CancellationToken>._))
                .MustHaveHappened();
        }

        private void MustLogWarning()
        {
            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }
    }
}
