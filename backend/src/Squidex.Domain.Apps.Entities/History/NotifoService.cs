// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.SDK;
using Notifo.Services;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using static Notifo.Services.Notifications;

namespace Squidex.Domain.Apps.Entities.History
{
    public class NotifoService : IInitializable, IUserEventHandler
    {
        private static readonly Duration MaxAge = Duration.FromHours(12);
        private readonly NotifoOptions options;
        private readonly IUrlGenerator urlGenerator;
        private readonly IClock clock;
        private NotificationsClient? client;

        public NotifoService(IOptions<NotifoOptions> options, IUrlGenerator urlGenerator, IClock clock)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.options = options.Value;
            this.urlGenerator = urlGenerator;
            this.clock = clock;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            if (options.IsConfigured())
            {
                var builder =
                    NotificationsClientBuilder.Create()
                        .SetApiKey(options.ApiKeyOwner);

                if (!string.IsNullOrWhiteSpace(options.ApiUrl))
                {
                    builder = builder.SetApiUrl(options.ApiUrl);
                }

                client = builder.Build();
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Claim>> OnUserRegisteringAsync(IUser user)
        {
            if (client == null)
            {
                return Enumerable.Empty<Claim>();
            }

            var userRequest = new UpsertUserRequest();
            userRequest.UserId = user.Id;
            userRequest.EmailAddress = user.Email;
            userRequest.FullName = user.DisplayName();

            var userResponse = await client.UpsertUserAsync(userRequest);

            var token = userResponse.User.ApiKey;

            return Enumerable.Repeat(new Claim(SquidexClaimTypes.NotifoKey, token), 1);
        }

        public async Task PublishAsync(Envelope<CommentCreated> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            if (client == null)
            {
                return;
            }

            if (IsTooOld(@event.Headers))
            {
                return;
            }

            var comment = @event.Payload;

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

                        SetUser(comment, publishRequest);

                        await stream.RequestStream.WriteAsync(publishRequest);
                    }

                    await stream.RequestStream.CompleteAsync();
                    await stream.ResponseAsync;
                }
            }
        }

        public async Task PublishAsync(Envelope<AppEvent> @event, HistoryEvent historyEvent)
        {
            if (client == null)
            {
                return;
            }

            if (IsTooOld(@event.Headers))
            {
                return;
            }

            var appEvent = @event.Payload;

            var publishRequest = new PublishRequest
            {
                AppId = options.AppId
            };

            foreach (var (key, value) in historyEvent.Parameters)
            {
                publishRequest.Properties.Add(key, value);
            }

            publishRequest.Properties["SquidexApp"] = appEvent.AppId.Name;

            if (appEvent is ContentEvent c && !(appEvent is ContentDeleted))
            {
                var url = urlGenerator.ContentUI(c.AppId, c.SchemaId, c.ContentId);

                publishRequest.Properties["SquidexUrl"] = url;
            }

            publishRequest.TemplateCode = historyEvent.EventType;

            SetUser(appEvent, publishRequest);
            SetTopic(appEvent, publishRequest, historyEvent);

            await client.PublishAsync(publishRequest);
        }

        private bool IsTooOld(EnvelopeHeaders headers)
        {
            var now = clock.GetCurrentInstant();

            return now - headers.Timestamp() > MaxAge;
        }

        private static void SetUser(AppEvent appEvent, PublishRequest publishRequest)
        {
            if (appEvent.Actor.IsSubject)
            {
                publishRequest.CreatorId = appEvent.Actor.Identifier;
            }
        }

        private static void SetTopic(AppEvent appEvent, PublishRequest publishRequest, HistoryEvent @event)
        {
            var topic = $"apps/{appEvent.AppId.Id}/{@event.Channel.Replace('.', '/').Trim()}";

            publishRequest.Topic = topic;
        }
    }
}
