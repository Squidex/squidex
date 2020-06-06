// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Notifo.SDK;
using Notifo.Services;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using static Notifo.Services.Notifications;

namespace Squidex.Domain.Apps.Entities.History
{
    public class NotifoService : IInitializable
    {
        private readonly NotifoOptions options;
        private readonly IUrlGenerator urlGenerator;
        private NotificationsClient? client;

        public NotifoService(IOptions<NotifoOptions> options, IUrlGenerator urlGenerator)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.options = options.Value;

            this.urlGenerator = urlGenerator;
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

                        if (comment.Url?.IsAbsoluteUri == true)
                        {
                            publishRequest.Preformatted.LinkUrl["en"] = comment.Url.ToString();
                        }

                        SetCreator(comment, publishRequest);

                        await stream.RequestStream.WriteAsync(publishRequest);
                    }

                    await stream.RequestStream.CompleteAsync();
                    await stream.ResponseAsync;
                }
            }
        }

        public async Task PublishAsync(AppEvent appEvent, HistoryEvent @event)
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

            publishRequest.Properties["SquidexApp"] = appEvent.AppId.Name;

            if (appEvent is ContentEvent c && !(appEvent is ContentDeleted))
            {
                var url = urlGenerator.ContentUI(c.AppId, c.SchemaId, c.ContentId);

                publishRequest.Properties["SquidexUrl"] = url;
            }

            publishRequest.TemplateCode = @event.EventType;

            SetCreator(appEvent, publishRequest);
            SetTopic(appEvent, @event, publishRequest);

            await client.PublishAsync(publishRequest);
        }

        private static void SetCreator(AppEvent appEvent, PublishRequest publishRequest)
        {
            if (appEvent.Actor.IsSubject)
            {
                publishRequest.CreatorId = appEvent.Actor.Identifier;
            }
        }

        private static void SetTopic(AppEvent appEvent, HistoryEvent @event, PublishRequest publishRequest)
        {
            var topic = $"apps/{appEvent.AppId.Id}/{@event.Channel.Replace('.', '/').Trim()}";

            publishRequest.Topic = topic;
        }
    }
}
