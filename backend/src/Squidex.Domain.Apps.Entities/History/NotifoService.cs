// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.SDK;
using Notifo.Services;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;
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
        private readonly IUserResolver userResolver;
        private readonly IClock clock;
        private NotificationsClient? client;

        public NotifoService(IOptions<NotifoOptions> options, IUrlGenerator urlGenerator, IUserResolver userResolver, IClock clock)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(userResolver, nameof(userResolver));
            Guard.NotNull(clock, nameof(clock));

            this.options = options.Value;

            this.urlGenerator = urlGenerator;
            this.userResolver = userResolver;

            this.clock = clock;
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

        public void OnUserUpdated(IUser user)
        {
            UpsertUserAsync(user).Forget();
        }

        private async Task UpsertUserAsync(IUser user)
        {
            if (client == null)
            {
                return;
            }

            var settings = new NotificationSettingsDto();

            settings.Channels[Providers.WebPush] = new NotificationSettingDto
            {
                Send = true,
                DelayInSeconds = null
            };

            settings.Channels[Providers.Email] = new NotificationSettingDto
            {
                Send = true,
                DelayInSeconds = 5 * 60
            };

            var userRequest = new UpsertUserRequest
            {
                AppId = options.AppId,
                EmailAddress = user.Email,
                FullName = user.DisplayName(),
                PreferredLanguage = "en",
                PreferredTimezone = null,
                RequiresWhitelistedTopic = true,
                Settings = settings,
                UserId = user.Id
            };

            var response = await client.UpsertUserAsync(userRequest);

            await userResolver.SetClaimAsync(user.Id, SquidexClaimTypes.NotifoKey, response.User.ApiKey);
        }

        public async Task HandleEventAsync(Envelope<IEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            if (client == null)
            {
                return;
            }

            switch (@event.Payload)
            {
                case CommentCreated comment:
                    {
                        if (IsTooOld(@event.Headers))
                        {
                            return;
                        }

                        if (comment.Mentions == null || comment.Mentions.Length == 0)
                        {
                            break;
                        }

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

                        break;
                    }

                case AppContributorAssigned contributorAssigned:
                    {
                        var user = await userResolver.FindByIdAsync(contributorAssigned.ContributorId);

                        if (user != null)
                        {
                            await UpsertUserAsync(user);
                        }

                        var request = BuildAllowedTopicRequest(contributorAssigned, contributorAssigned.ContributorId);

                        await client.AddAllowedTopicAsync(request);

                        break;
                    }

                case AppContributorRemoved contributorRemoved:
                    {
                        var request = BuildAllowedTopicRequest(contributorRemoved, contributorRemoved.ContributorId);

                        await client.RemoveAllowedTopicAsync(request);

                        break;
                    }
            }
        }

        private AllowedTopicRequest BuildAllowedTopicRequest(AppEvent @event, string contributorId)
        {
            var topicRequest = new AllowedTopicRequest
            {
                AppId = options.AppId
            };

            topicRequest.UserId = contributorId;
            topicRequest.TopicPrefix = GetAppPrefix(@event);

            return topicRequest;
        }

        public async Task HandleHistoryEventAsync(Envelope<AppEvent> @event, HistoryEvent historyEvent)
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
            var topicPrefix = GetAppPrefix(appEvent);
            var topicSuffix = @event.Channel.Replace('.', '/').Trim();

            publishRequest.Topic = $"{topicPrefix}/{topicSuffix}";
        }

        private static string GetAppPrefix(AppEvent appEvent)
        {
            return $"apps/{appEvent.AppId.Id}";
        }
    }
}
