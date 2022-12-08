// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Email;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Notifications;

public class EmailUserNotificationsTests
{
    private readonly IEmailSender emailSender = A.Fake<IEmailSender>();
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IUser assigner = UserMocks.User("1", "1@email.com", "user1");
    private readonly IUser assigned = UserMocks.User("2", "2@email.com", "user2");
    private readonly ILogger<EmailUserNotifications> log = A.Fake<ILogger<EmailUserNotifications>>();
    private readonly IAppEntity app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));
    private readonly ITeamEntity team = Mocks.Team(DomainId.NewGuid());
    private readonly EmailUserNotificationOptions texts = new EmailUserNotificationOptions();
    private readonly EmailUserNotifications sut;

    public EmailUserNotificationsTests()
    {
        A.CallTo(() => urlGenerator.UI())
            .Returns("my-ui");

        sut = new EmailUserNotifications(Options.Create(texts), emailSender, urlGenerator, log);
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
    public async Task Should_not_send_app_invitation_email_if_texts_for_new_user_are_empty()
    {
        await sut.SendInviteAsync(assigner, assigned, app);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_text_invitation_email_if_texts_for_new_user_are_empty()
    {
        await sut.SendInviteAsync(assigner, assigned, team);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_app_invitation_email_if_texts_for_existing_user_are_empty()
    {
        await sut.SendInviteAsync(assigner, assigned, app);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_text_invitation_email_if_texts_for_existing_user_are_empty()
    {
        await sut.SendInviteAsync(assigner, assigned, team);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_usage_email_if_texts_empty()
    {
        await sut.SendUsageAsync(assigned, app, 100, 120);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        MustLogWarning();
    }

    [Fact]
    public async Task Should_not_send_app_invitation_email_if_no_consent_given()
    {
        var withoutConsent = UserMocks.User("2", "2@email.com", "user", false);

        texts.ExistingUserSubject = "email-subject";
        texts.ExistingUserBody = "email-body";

        await sut.SendInviteAsync(assigner, withoutConsent, app);

        A.CallTo(() => emailSender.SendAsync(withoutConsent.Email, "email-subject", "email-body", A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_send_team_invitation_email_if_no_consent_given()
    {
        var withoutConsent = UserMocks.User("2", "2@email.com", "user", false);

        texts.ExistingTeamUserSubject = "email-subject";
        texts.ExistingTeamUserBody = "email-body";

        await sut.SendInviteAsync(assigner, withoutConsent, team);

        A.CallTo(() => emailSender.SendAsync(withoutConsent.Email, "email-subject", "email-body", A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_send_app_invitation_email_if_consent_given()
    {
        var withConsent = UserMocks.User("2", "2@email.com", "user", true);

        texts.ExistingUserSubject = "email-subject";
        texts.ExistingUserBody = "email-body";

        await sut.SendInviteAsync(assigner, withConsent, app);

        A.CallTo(() => emailSender.SendAsync(withConsent.Email, "email-subject", "email-body", A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_team_invitation_email_if_consent_given()
    {
        var withConsent = UserMocks.User("2", "2@email.com", "user", true);

        texts.ExistingTeamUserSubject = "email-subject";
        texts.ExistingTeamUserBody = "email-body";

        await sut.SendInviteAsync(assigner, withConsent, team);

        A.CallTo(() => emailSender.SendAsync(withConsent.Email, "email-subject", "email-body", A<CancellationToken>._))
            .MustHaveHappened();
    }

    private async Task TestUsageFormattingAsync(string pattern, string actual)
    {
        texts.UsageSubject = pattern;
        texts.UsageBody = pattern;

        await sut.SendUsageAsync(assigned, app, 100, 120);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, actual, actual, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private async Task TestInvitationFormattingAsync(string pattern, string actual)
    {
        texts.NewUserSubject = pattern;
        texts.NewUserBody = pattern;

        await sut.SendInviteAsync(assigner, assigned, app);

        A.CallTo(() => emailSender.SendAsync(assigned.Email, actual, actual, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private void MustLogWarning()
    {
        A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappened();
    }
}
