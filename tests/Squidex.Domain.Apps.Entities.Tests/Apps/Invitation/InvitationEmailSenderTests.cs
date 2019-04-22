// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Email;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public class InvitationEmailSenderTests
    {
        private readonly IEmailSender emailSender = A.Fake<IEmailSender>();
        private readonly IEmailUrlGenerator emailUrlGenerator = A.Fake<IEmailUrlGenerator>();
        private readonly IUser assigner = A.Fake<IUser>();
        private readonly IUser assignee = A.Fake<IUser>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly string appName = "my-app";
        private readonly string uiUrl = "my-ui";
        private readonly InvitationEmailTextOptions texts = new InvitationEmailTextOptions();
        private readonly InvitationEmailSender sut;

        public InvitationEmailSenderTests()
        {
            A.CallTo(() => assigner.Email)
                .Returns("sebastian@squidex.io");
            A.CallTo(() => assigner.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Sebastian Stehle") });

            A.CallTo(() => assignee.Email)
                .Returns("qaisar@squidex.io");
            A.CallTo(() => assignee.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Qaisar Ahmad") });

            A.CallTo(() => emailUrlGenerator.GenerateUIUrl())
                .Returns(uiUrl);

            sut = new InvitationEmailSender(Options.Create(texts), emailSender, emailUrlGenerator, log);
        }

        [Fact]
        public async Task Should_format_assigner_email_and_send_email()
        {
            await TestFormattingAsync("Email: $ASSIGNER_EMAIL", "Email: sebastian@squidex.io");
        }

        [Fact]
        public async Task Should_format_assignee_email_and_send_email()
        {
            await TestFormattingAsync("Email: $ASSIGNEE_EMAIL", "Email: qaisar@squidex.io");
        }

        [Fact]
        public async Task Should_format_assigner_name_and_send_email()
        {
            await TestFormattingAsync("Name: $ASSIGNER_NAME", "Name: Sebastian Stehle");
        }

        [Fact]
        public async Task Should_format_assignee_name_and_send_email()
        {
            await TestFormattingAsync("Name: $ASSIGNEE_NAME", "Name: Qaisar Ahmad");
        }

        [Fact]
        public async Task Should_format_app_name_and_send_email()
        {
            await TestFormattingAsync("App: $APP_NAME", "App: my-app");
        }

        [Fact]
        public async Task Should_format_ui_url_and_send_email()
        {
            await TestFormattingAsync("UI: $UI_URL", "UI: my-ui");
        }

        [Fact]
        public async Task Should_not_send_email_if_texts_for_new_user_are_empty()
        {
            await sut.SendNewUserEmailAsync(assigner, assignee, appName);

            A.CallTo(() => emailSender.SendAsync(assignee.Email, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_not_send_email_if_texts_for_existing_user_are_empty()
        {
            await sut.SendExistingUserEmailAsync(assigner, assignee, appName);

            A.CallTo(() => emailSender.SendAsync(assignee.Email, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();

            MustLogWarning();
        }

        [Fact]
        public async Task Should_send_email_for_existing_user()
        {
            texts.ExistingUserSubject = "email-subject";
            texts.ExistingUserBody = "email-body";

            await sut.SendExistingUserEmailAsync(assigner, assignee, appName);

            A.CallTo(() => emailSender.SendAsync(assignee.Email, "email-subject", "email-body"))
                .MustHaveHappened();
        }

        private async Task TestFormattingAsync(string pattern, string result)
        {
            texts.NewUserSubject = pattern;
            texts.NewUserBody = pattern;

            await sut.SendNewUserEmailAsync(assigner, assignee, appName);

            A.CallTo(() => emailSender.SendAsync(assignee.Email, result, result))
                .MustHaveHappened();
        }

        private void MustLogWarning()
        {
            A.CallTo(() => log.Log(SemanticLogLevel.Warning, A<None>.Ignored, A<Action<None, IObjectWriter>>.Ignored))
                .MustHaveHappened();
        }
    }
}
