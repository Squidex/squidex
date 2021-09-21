// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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
using Squidex.Log;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

#pragma warning disable MA0073 // Avoid comparison with bool constant

namespace Squidex.Domain.Apps.Entities.History
{
    public class NotifoService : IUserEvents
    {
        private static readonly Duration MaxAge = Duration.FromHours(12);
        private readonly NotifoOptions options;
        private readonly IUrlGenerator urlGenerator;
        private readonly IUserResolver userResolver;
        private readonly ISemanticLog log;
        private readonly IClock clock;
        private readonly INotifoClient? client;

        public NotifoService(IOptions<NotifoOptions> options,
            IUrlGenerator urlGenerator,
            IUserResolver userResolver,
            ISemanticLog log,
            IClock clock)
        {
            this.options = options.Value;

            this.urlGenerator = urlGenerator;
            this.userResolver = userResolver;
            this.clock = clock;

            this.log = log;

            if (options.Value.IsConfigured())
            {
                var builder =
                    NotifoClientBuilder.Create()
                        .SetApiKey(options.Value.ApiKey);

                if (!string.IsNullOrWhiteSpace(options.Value.ApiUrl))
                {
                    builder = builder.SetApiUrl(options.Value.ApiUrl);
                }

                if (options.Value.Debug)
                {
                    builder = builder.ReadResponseAsString(true);
                }

                client = builder.Build();
            }
        }

        public async Task OnUserCreatedAsync(IUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await UpsertUserAsync(user);
            }
        }

        public async Task OnUserUpdatedAsync(IUser user, IUser previous)
        {
            if (!string.Equals(user.Email, previous?.Email, StringComparison.OrdinalIgnoreCase))
            {
                await UpsertUserAsync(user);
            }
        }

        private async Task UpsertUserAsync(IUser user)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                var settings = new Dictionary<string, NotificationSettingDto>
                {
                    [Providers.WebPush] = new NotificationSettingDto
                    {
                        Send = NotificationSend.Send,
                        DelayInSeconds = null
                    },

                    [Providers.Email] = new NotificationSettingDto
                    {
                        Send = NotificationSend.Send,
                        DelayInSeconds = 5 * 60
                    }
                };

                var userRequest = new UpsertUserDto
                {
                    Id = user.Id,
                    FullName = user.Claims.DisplayName(),
                    PreferredLanguage = "en",
                    PreferredTimezone = null,
                    Settings = settings
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

                var apiKey = response.First().ApiKey;

                await userResolver.SetClaimAsync(user.Id, SquidexClaimTypes.NotifoKey, response.First().ApiKey, true);
            }
            catch (NotifoException ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "RegisterToNotifo")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("details", ex.ToString()));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "RegisterToNotifo")
                    .WriteProperty("status", "Failed"));
            }
        }

        public async Task HandleEventsAsync(IEnumerable<(Envelope<AppEvent> AppEvent, HistoryEvent? HistoryEvent)> events)
        {
            Guard.NotNull(events, nameof(events));

            if (client == null)
            {
                return;
            }

            try
            {
                var now = clock.GetCurrentInstant();

                var maxAge = now - MaxAge;

                var batches = events
                    .Where(x => x.AppEvent.Headers.Restored() == false)
                    .Where(x => x.AppEvent.Headers.Timestamp() > maxAge)
                    .SelectMany(x => CreateRequests(x.AppEvent, x.HistoryEvent))
                    .Batch(50);

                foreach (var batch in batches)
                {
                    var request = new PublishManyDto
                    {
                        Requests = batch.ToList()
                    };

                    await client.Events.PostEventsAsync(options.AppId, request);
                }

                foreach (var @event in events)
                {
                    switch (@event.AppEvent.Payload)
                    {
                        case AppContributorAssigned contributorAssigned:
                            await AssignContributorAsync(client, contributorAssigned);
                            break;

                        case AppContributorRemoved contributorRemoved:
                            await RemoveContributorAsync(client, contributorRemoved);
                            break;
                    }
                }
            }
            catch (NotifoException ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "RegisterToNotifo")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("details", ex.ToString()));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "RegisterToNotifo")
                    .WriteProperty("status", "Failed"));
            }
        }

        private async Task AssignContributorAsync(INotifoClient actualClient, AppContributorAssigned contributorAssigned)
        {
            var userId = contributorAssigned.ContributorId;

            var user = await userResolver.FindByIdAsync(userId);

            if (user != null)
            {
                await UpsertUserAsync(user);
            }

            try
            {
                var request = new AddAllowedTopicDto
                {
                    Prefix = GetAppPrefix(contributorAssigned)
                };

                await actualClient.Users.PostAllowedTopicAsync(options.AppId, userId, request);
            }
            catch (NotifoException ex) when (ex.StatusCode != 404)
            {
                throw;
            }
        }

        private async Task RemoveContributorAsync(INotifoClient actualClient, AppContributorRemoved contributorRemoved)
        {
            var userId = contributorRemoved.ContributorId;

            try
            {
                var prefix = GetAppPrefix(contributorRemoved);

                await actualClient.Users.DeleteAllowedTopicAsync(options.ApiKey, userId, prefix);
            }
            catch (NotifoException ex) when (ex.StatusCode != 404)
            {
                throw;
            }
        }

        private IEnumerable<PublishDto> CreateRequests(Envelope<AppEvent> appEvent, HistoryEvent? historyEvent)
        {
            if (appEvent.Payload is CommentCreated comment && comment.Mentions?.Length > 0)
            {
                foreach (var userId in comment.Mentions)
                {
                    yield return CreateMentionRequest(comment, userId);
                }
            }
            else if (historyEvent != null)
            {
                yield return CreateHistoryRequest(historyEvent, appEvent.Payload);
            }
        }

        private PublishDto CreateHistoryRequest(HistoryEvent historyEvent, AppEvent payload)
        {
            var publishRequest = new PublishDto
            {
                Properties = new EventProperties()
            };

            foreach (var (key, value) in historyEvent.Parameters)
            {
                publishRequest.Properties.Add(key, value);
            }

            publishRequest.Properties["SquidexApp"] = payload.AppId.Name;

            if (payload is ContentEvent @event && payload is not ContentDeleted)
            {
                var url = urlGenerator.ContentUI(@event.AppId, @event.SchemaId, @event.ContentId);

                publishRequest.Properties["SquidexUrl"] = url;
            }

            publishRequest.TemplateCode = historyEvent.EventType;

            SetUser(payload, publishRequest);
            SetTopic(payload, publishRequest, historyEvent);

            return publishRequest;
        }

        private static PublishDto CreateMentionRequest(CommentCreated comment, string userId)
        {
            var publishRequest = new PublishDto
            {
                Topic = $"users/{userId}"
            };

            publishRequest.Properties["SquidexApp"] = comment.AppId.Name;

            publishRequest.Preformatted = new NotificationFormattingDto
            {
                Subject =
                {
                    ["en"] = comment.Text
                }
            };

            if (comment.Url?.IsAbsoluteUri == true)
            {
                publishRequest.Preformatted.LinkUrl["en"] = comment.Url.ToString();
            }

            SetUser(comment, publishRequest);

            return publishRequest;
        }

        private static void SetUser(AppEvent appEvent, PublishDto publishRequest)
        {
            if (appEvent.Actor.IsUser)
            {
                publishRequest.CreatorId = appEvent.Actor.Identifier;
            }
        }

        private static void SetTopic(AppEvent appEvent, PublishDto publishRequest, HistoryEvent @event)
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
