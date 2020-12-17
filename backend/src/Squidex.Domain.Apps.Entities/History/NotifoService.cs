// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.SDK;
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

namespace Squidex.Domain.Apps.Entities.History
{
    public class NotifoService : IInitializable, IUserEventHandler
    {
        private static readonly Duration MaxAge = Duration.FromHours(12);
        private readonly NotifoOptions options;
        private readonly IUrlGenerator urlGenerator;
        private readonly IUserResolver userResolver;
        private readonly IClock clock;
        private INotifoClient? client;

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
                    NotifoClientBuilder.Create()
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

            settings[Providers.WebPush] = new NotificationSettingDto
            {
                Send = true,
                DelayInSeconds = null
            };

            settings[Providers.Email] = new NotificationSettingDto
            {
                Send = true,
                DelayInSeconds = 5 * 60
            };

            var userRequest = new UpsertUserDto
            {
                Id = user.Id,
                FullName = user.DisplayName(),
                PreferredLanguage = "en",
                PreferredTimezone = null,
                Settings = settings,
            };

            if (user.Email.IsEmail())
            {
                userRequest.EmailAddress = user.Email;
            }

            var response = await client.Users.PostUsersAsync(options.AppId, new UpsertUsersDto
            {
                Requests = new List<UpsertUserDto>
                {
                    userRequest
                }
            });

            await userResolver.SetClaimAsync(user.Id, SquidexClaimTypes.NotifoKey, response.First().ApiKey);
        }

        public async Task HandleEventsAsync(IEnumerable<(Envelope<AppEvent> AppEvent, HistoryEvent? HistoryEvent)> events)
        {
            Guard.NotNull(events, nameof(events));

            if (client == null)
            {
                return;
            }

            var now = clock.GetCurrentInstant();

            var publishedEvents = events
                .Where(x => IsTooOld(x.AppEvent.Headers, now) == false)
                .Where(x => IsComment(x.AppEvent.Payload) || x.HistoryEvent != null)
                .ToList();

            if (publishedEvents.Any())
            {
                var requests = new List<PublishRequestDto>();

                foreach (var @event in publishedEvents)
                {
                    var payload = @event.AppEvent.Payload;

                    if (payload is CommentCreated comment && IsComment(payload))
                    {
                        foreach (var userId in comment.Mentions!)
                        {
                            var publishRequest = new PublishRequestDto();

                            publishRequest.Topic = $"users/{userId}";

                            publishRequest.Properties["SquidexApp"] = comment.AppId.Name;
                            publishRequest.Preformatted = new NotificationFormattingDto();
                            publishRequest.Preformatted.Subject["en"] = comment.Text;

                            if (comment.Url?.IsAbsoluteUri == true)
                            {
                                publishRequest.Preformatted.LinkUrl["en"] = comment.Url.ToString();
                            }

                            SetUser(comment, publishRequest);

                            requests.Add(publishRequest);
                        }
                    }
                    else if (@event.HistoryEvent != null)
                    {
                        var historyEvent = @event.HistoryEvent;

                        var publishRequest = new PublishRequestDto
                        {
                            Properties = new EventProperties()
                        };

                        foreach (var (key, value) in historyEvent.Parameters)
                        {
                            publishRequest.Properties.Add(key, value);
                        }

                        publishRequest.Properties["SquidexApp"] = payload.AppId.Name;

                        if (payload is ContentEvent c && !(payload is ContentDeleted))
                        {
                            var url = urlGenerator.ContentUI(c.AppId, c.SchemaId, c.ContentId);

                            publishRequest.Properties["SquidexUrl"] = url;
                        }

                        publishRequest.TemplateCode = @event.HistoryEvent.EventType;

                        SetUser(payload, publishRequest);
                        SetTopic(payload, publishRequest, historyEvent);

                        requests.Add(publishRequest);
                    }
                }

                await client.Events.PostEventsAsync(options.AppId, new PublishManyRequestDto { Requests = requests });
            }

            foreach (var @event in events)
            {
                switch (@event.AppEvent.Payload)
                {
                    case AppContributorAssigned contributorAssigned:
                        {
                            var userId = contributorAssigned.ContributorId;

                            var user = await userResolver.FindByIdAsync(userId);

                            if (user != null)
                            {
                                await UpsertUserAsync(user);
                            }

                            try
                            {
                                await client.Users.PostAllowedTopicAsync(options.AppId, userId, new AddAllowedTopicRequest
                                {
                                    Prefix = GetAppPrefix(contributorAssigned)
                                });
                            }
                            catch (NotifoException ex) when (ex.StatusCode == 404)
                            {
                                break;
                            }

                            break;
                        }

                    case AppContributorRemoved contributorRemoved:
                        {
                            var userId = contributorRemoved.ContributorId;

                            try
                            {
                                var prefix = GetAppPrefix(contributorRemoved);

                                await client.Users.DeleteAllowedTopicAsync(options.ApiKey, userId, prefix);
                            }
                            catch (NotifoException ex) when (ex.StatusCode == 404)
                            {
                                break;
                            }

                            break;
                        }
                }
            }
        }

        private static bool IsTooOld(EnvelopeHeaders headers, Instant now)
        {
            return now - headers.Timestamp() > MaxAge;
        }

        private static bool IsComment(AppEvent appEvent)
        {
            return appEvent is CommentCreated comment && comment.Mentions?.Length > 0;
        }

        private static void SetUser(AppEvent appEvent, PublishRequestDto publishRequest)
        {
            if (appEvent.Actor.IsSubject)
            {
                publishRequest.CreatorId = appEvent.Actor.Identifier;
            }
        }

        private static void SetTopic(AppEvent appEvent, PublishRequestDto publishRequest, HistoryEvent @event)
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
