// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Email;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public sealed class InvitationEmailSender : IInvitationEmailSender
    {
        private readonly IEmailSender emailSender;
        private readonly IEmailUrlGenerator emailUrlGenerator;
        private readonly ISemanticLog log;
        private readonly InvitationEmailTextOptions texts;

        public bool IsActive
        {
            get { return true; }
        }

        public InvitationEmailSender(
            IOptions<InvitationEmailTextOptions> texts,
            IEmailSender emailSender,
            IEmailUrlGenerator emailUrlGenerator,
            ISemanticLog log)
        {
            Guard.NotNull(texts, nameof(texts));
            Guard.NotNull(emailSender, nameof(emailSender));
            Guard.NotNull(emailUrlGenerator, nameof(emailUrlGenerator));
            Guard.NotNull(log, nameof(log));

            this.texts = texts.Value;
            this.emailSender = emailSender;
            this.emailUrlGenerator = emailUrlGenerator;
            this.log = log;
        }

        public Task SendExistingUserEmailAsync(IUser assigner, IUser assignee, string appName)
        {
            return SendEmailAsync(texts.ExistingUserBody, texts.ExistingUserSubject, assigner, assignee, appName);
        }

        public Task SendNewUserEmailAsync(IUser assigner, IUser assignee, string appName)
        {
            return SendEmailAsync(texts.NewUserSubject, texts.NewUserBody, assigner, assignee, appName);
        }

        private async Task SendEmailAsync(string emailSubj, string emailBody, IUser assigner, IUser assignee, string appName)
        {
            if (string.IsNullOrWhiteSpace(emailBody))
            {
                LogWarning("No email subject configured for new users");
                return;
            }

            if (string.IsNullOrWhiteSpace(emailSubj))
            {
                LogWarning("No email body configured for new users");
                return;
            }

            var appUrl = emailUrlGenerator.GenerateUIUrl();

            emailSubj = Format(emailSubj, assigner, assignee, appUrl, appName);
            emailBody = Format(emailBody, assigner, assignee, appUrl, appName);

            await emailSender.SendAsync(assignee.Email, emailSubj, emailBody);
        }

        private void LogWarning(string reason)
        {
            log.LogWarning(w => w
                .WriteProperty("action", "InviteUser")
                .WriteProperty("status", "Failed")
                .WriteProperty("reason", reason));
        }

        private string Format(string text, IUser assigner, IUser assignee, string uiUrl, string appName)
        {
            text = text.Replace("$APP_NAME", appName);

            text = text.Replace("$UI_URL", uiUrl);

            if (assigner != null)
            {
                text = text.Replace("$ASSIGNER_EMAIL", assigner.Email);
                text = text.Replace("$ASSIGNER_NAME", assigner.DisplayName());
            }

            if (assignee != null)
            {
                text = text.Replace("$ASSIGNEE_EMAIL", assignee.Email);
                text = text.Replace("$ASSIGNEE_NAME", assignee.DisplayName());
            }

            return text;
        }
    }
}
