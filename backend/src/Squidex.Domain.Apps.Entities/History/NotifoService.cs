// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Notifo.SDK;
using Notifo.Services;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using static Notifo.Services.Notifications;

namespace Squidex.Domain.Apps.Entities.History
{
    public class NotifoService : IInitializable
    {
        private readonly NotifoOptions options;
        private NotificationsClient? client;

        public NotifoService(IOptions<NotifoOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            this.options = options.Value;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            if (options.IsConfigured())
            {
                var builder =
                    NotificationsClientBuilder.Create()
                        .SetApiKey(options.ApiKey);

                if (!string.IsNullOrWhiteSpace(options.ApiUrl))
                {
                    builder = builder.SetApiUrl(options.ApiUrl);
                }

                client = builder.Build();
            }

            return Task.CompletedTask;
        }

        public async Task PublishAsync(CommentCreated comment)
        {
            if (client == null)
            {
                return;
            }

            if (comment.Mentions != null && comment.Mentions.Length > 0)
            {
                using (var stream = client.PublishMany())
                {
                    foreach (var userId in comment.Mentions)
                    {
                        var publishRequest = new PublishRequest
                        {
                            AppId = options.AppId
                        };

                        publishRequest.Topic = $"users/{userId}";

                        publishRequest.Properties["SquidexApp"] = comment.AppId.Name;
                        publishRequest.Preformatted = new NotificationFormattingDto();
                        publishRequest.Preformatted.Subject["en"] = comment.Text;

                        publishRequest.CreatorId = comment.Actor.Identifier;

                        await stream.RequestStream.WriteAsync(publishRequest);
                    }

                    await stream.RequestStream.CompleteAsync();
                    await stream.ResponseAsync;
                }
            }
        }

        public async Task PublishAsync(NamedId<Guid> appId, string userId, HistoryEvent @event)
        {
            if (client == null)
            {
                return;
            }

            var publishRequest = new PublishRequest
            {
                AppId = options.AppId
            };

            foreach (var (key, value) in @event.Parameters)
            {
                publishRequest.Properties.Add(key, value);
            }

            publishRequest.Properties["SquidexApp"] = appId.Name;
            publishRequest.TemplateCode = @event.EventType;
            publishRequest.CreatorId = userId;

            SetTopic(appId.Id, @event, publishRequest);

            await client.PublishAsync(publishRequest);
        }

        private static void SetTopic(Guid appId, HistoryEvent @event, PublishRequest publishRequest)
        {
            var topic = $"apps/{appId}/{@event.Channel.Replace('.', '/').Trim()}";

            publishRequest.Topic = topic;
        }
    }
}
