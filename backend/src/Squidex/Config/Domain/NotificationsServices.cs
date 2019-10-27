// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.History.Notifications;
using Squidex.Infrastructure.Email;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain
{
    public static class NotificationsServices
    {
        public static void AddSquidexNotifications(this IServiceCollection services, IConfiguration config)
        {
            var emailOptions = config.GetSection("email:smtp").Get<SmptOptions>();

            if (emailOptions.IsConfigured())
            {
                services.AddSingleton(Options.Create(emailOptions));

                services.Configure<NotificationEmailTextOptions>(
                    config.GetSection("email:notifications"));

                services.AddSingletonAs<SmtpEmailSender>()
                    .As<IEmailSender>();

                services.AddSingletonAs<NotificationEmailSender>()
                    .AsOptional<INotificationEmailSender>();
            }
            else
            {
                services.AddSingletonAs<NoopNotificationEmailSender>()
                    .AsOptional<INotificationEmailSender>();
            }

            services.AddSingletonAs<NotificationEmailEventConsumer>()
                .As<IEventConsumer>();
        }
    }
}