// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Invitation;
using Squidex.Domain.Apps.Entities.Notifications;
using Squidex.Infrastructure.Email;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain;

public static class NotificationsServices
{
    public static void AddSquidexNotifications(this IServiceCollection services, IConfiguration config)
    {
        var emailOptions = config.GetSection("email:smtp").Get<SmtpOptions>() ?? new ();

        if (emailOptions.IsConfigured())
        {
            services.AddSingleton(Options.Create(emailOptions));

            services.Configure<EmailUserNotificationOptions>(config,
                "email:notifications");

            services.AddSingletonAs<SmtpEmailSender>()
                .As<IEmailSender>();

            services.AddSingletonAs<EmailUserNotifications>()
                .AsOptional<IUserNotifications>();
        }
        else
        {
            services.AddSingletonAs<NoopUserNotifications>()
                .AsOptional<IUserNotifications>();
        }

        services.AddSingletonAs<InvitationEventConsumer>()
            .As<IEventConsumer>();
    }
}
