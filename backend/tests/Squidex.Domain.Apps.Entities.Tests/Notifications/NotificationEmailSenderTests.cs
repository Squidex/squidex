// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.Email;
using Squidex.Log;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Notifications
{
    public class NotificationEmailSenderTests
    {
        private readonly IEmailSender emailSender = A.Fake<IEmailSender>();
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly IUser assigner = A.Fake<IUser>();
        private readonly IUser user = A.Fake<IUser>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly List<Claim> assignerClaims = new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Sebastian Stehle") };
        private readonly List<Claim> assigneeClaims = new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Qaisar Ahmad") };
        private readonly string appName = "my-app";
        private readonly string uiUrl = "my-ui";
        private readonly NotificationEmailTextOptions texts = new NotificationEmailTextOptions();
        private readonly NotificationEmailSender sut;

        public NotificationEmailSenderTests()
        {
            A.CallTo(() => assigner.Email)
                .Returns("sebastian@squidex.io");
            A.CallTo(() => assigner.Claims)
                .Returns(assignerClaims);

            A.CallTo(() => user.Email)
                .Returns("qaisar@squidex.io");
            A.CallTo(() => user.Claims)
                .Returns(assigneeClaims);

            A.CallTo(() => urlGenerator.UI())
                .Returns(uiUrl);

            sut = new NotificationEmailSender(Options.Create(texts), emailSender, urlGenerator, log);
        }

        [Fact]
        public async Task Should_format_assigner_email_and_send_email()
        {
            await TestInvitationFormattingAsync("Email: $ASSIGNER_EMAIL", "Email: sebastian@squidex.io");
        }

        [Fact]
        public async Task Should_format_assigner_name_and_send_email()
        {
            await TestInvitationFormattingAsync("Name: $ASSIGNER_NAME", "Name: Sebastian Stehle");
        }

        [Fact]
        public async Task Should_format_user_email_and_send_email()
        {
            await TestInvitationFormattingAsync("Email: $USER_EMAIL", "Email: qaisar@squidex.io");
        }

        [Fact]
        public async Task Should_format_user_name_and_send_email()
        {
            await TestInvitationFormattingAsync("Name: $USER_NAME", "Name: Qaisar Ahmad");
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
            await sut.SendInviteAsync(assigner, user, appName);

            A.CallTo(() => emailSender.SendAsync(user.Email, A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_invitation_email_if_texts_for_existing_user_are_empty()
        {
            await sut.SendInviteAsync(assigner, user, appName);

            A.CallTo(() => emailSender.SendAsync(user.Email, A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_usage_email_if_texts_empty()
        {
            await sut.SendUsageAsync(user, appName, 100, 120);

            A.CallTo(() => emailSender.SendAsync(user.Email, A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_send_invitation_email_when_consent_given()
        {
            assigneeClaims.Add(new Claim(SquidexClaimTypes.Consent, "True"));

            texts.ExistingUserSubject = "email-subject";
            texts.ExistingUserBody = "email-body";

            await sut.SendInviteAsync(assigner, user, appName);

            A.CallTo(() => emailSender.SendAsync(user.Email, "email-subject", "email-body", A<CancellationToken>._))
                .MustHaveHappened();
        }

        private async Task TestUsageFormattingAsync(string pattern, string result)
        {
            texts.UsageSubject = pattern;
            texts.UsageBody = pattern;

            await sut.SendUsageAsync(user, appName, 100, 120);

            A.CallTo(() => emailSender.SendAsync(user.Email, result, result, A<CancellationToken>._))
                .MustHaveHappened();
        }

        private async Task TestInvitationFormattingAsync(string pattern, string result)
        {
            texts.NewUserSubject = pattern;
            texts.NewUserBody = pattern;

            await sut.SendInviteAsync(assigner, user, appName);

            A.CallTo(() => emailSender.SendAsync(user.Email, result, result, A<CancellationToken>._))
                .MustHaveHappened();
        }

        private void MustLogWarning()
        {
            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }
    }
}
