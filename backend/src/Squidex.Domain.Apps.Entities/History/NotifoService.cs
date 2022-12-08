// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.SDK;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

#pragma warning disable MA0073 // Avoid comparison with bool constant

namespace Squidex.Domain.Apps.Entities.History;

public class NotifoService : IUserEvents
{
    private static readonly Duration MaxAge = Duration.FromHours(12);
    private readonly NotifoOptions options;
    private readonly IUrlGenerator urlGenerator;
    private readonly IUserResolver userResolver;
    private readonly ILogger<NotifoService> log;
    private readonly INotifoClient? client;

    public IClock Clock { get; set; } = SystemClock.Instance;

    public NotifoService(IOptions<NotifoOptions> options,
        IUrlGenerator urlGenerator,
        IUserResolver userResolver,
        ILogger<NotifoService> log)
    {
        this.options = options.Value;
        this.urlGenerator = urlGenerator;
        this.userResolver = userResolver;
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
            var settings = new Dictionary<string, ChannelSettingDto>
            {
                [Providers.WebPush] = new ChannelSettingDto
                {
                    Send = ChannelSend.Send,
                    DelayInSeconds = null
                },

                [Providers.Email] = new ChannelSettingDto
                {
                    Send = ChannelSend.Send,
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
            log.LogError(ex, "Failed to register user in notifo: {details}.", ex.ToString());
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to register user in notifo.");
        }
    }

    public async Task HandleEventsAsync(IEnumerable<(Envelope<IEvent> AppEvent, HistoryEvent? HistoryEvent)> events)
    {
        Guard.NotNull(events);

        if (client == null)
        {
            return;
        }

        try
        {
            var now = Clock.GetCurrentInstant();

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
                    case AppContributorAssigned assigned:
                        await AssignContributorAsync(client, assigned.ContributorId, GetAppPrefix(assigned));
                        break;

                    case AppContributorRemoved removed:
                        await RemoveContributorAsync(client, removed.ContributorId, GetAppPrefix(removed));
                        break;

                    case TeamContributorAssigned assigned:
                        await AssignContributorAsync(client, assigned.ContributorId, GetTeamPrefix(assigned));
                        break;

                    case TeamContributorRemoved removed:
                        await RemoveContributorAsync(client, removed.ContributorId, GetTeamPrefix(removed));
                        break;
                }
            }
        }
        catch (NotifoException ex)
        {
            log.LogError(ex, "Failed to push user to notifo: {details}.", ex.ToString());
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to push user to notifo.");
        }
    }

    private async Task AssignContributorAsync(INotifoClient actualClient, string userId, string prefix)
    {
        var user = await userResolver.FindByIdAsync(userId);

        if (user != null)
        {
            await UpsertUserAsync(user);
        }

        try
        {
            var request = new AddAllowedTopicDto
            {
                Prefix = prefix
            };

            await actualClient.Users.PostAllowedTopicAsync(options.AppId, userId, request);
        }
        catch (NotifoException ex) when (ex.StatusCode != 404)
        {
            throw;
        }
    }

    private async Task RemoveContributorAsync(INotifoClient actualClient, string userId, string prefix)
    {
        try
        {
            await actualClient.Users.DeleteAllowedTopicAsync(options.ApiKey, userId, prefix);
        }
        catch (NotifoException ex) when (ex.StatusCode != 404)
        {
            throw;
        }
    }

    private IEnumerable<PublishDto> CreateRequests(Envelope<IEvent> @event, HistoryEvent? historyEvent)
    {
        if (@event.Payload is CommentCreated { Mentions.Length: > 0 } comment)
        {
            foreach (var userId in comment.Mentions)
            {
                yield return CreateMentionRequest(comment, userId);
            }
        }
        else if (historyEvent != null && @event.Payload is AppEvent appEvent)
        {
            yield return CreateHistoryRequest(historyEvent, appEvent);
        }
    }

    private PublishDto CreateHistoryRequest(HistoryEvent historyEvent, IEvent payload)
    {
        var publishRequest = new PublishDto
        {
            Properties = new NotificationProperties()
        };

        foreach (var (key, value) in historyEvent.Parameters)
        {
            publishRequest.Properties.Add(key, value);
        }

        if (payload is AppEvent appEvent)
        {
            publishRequest.Properties["SquidexApp"] = appEvent.AppId.Name;
        }

        if (payload is SquidexEvent squidexEvent)
        {
            SetUser(squidexEvent, publishRequest);
        }

        if (payload is AppEvent appEvent2)
        {
            publishRequest.Topic = BuildTopic(GetAppPrefix(appEvent2), historyEvent);
        }

        if (payload is TeamEvent teamEvent)
        {
            publishRequest.Topic = BuildTopic(GetTeamPrefix(teamEvent), historyEvent);
        }

        if (payload is ContentEvent @event and not ContentDeleted)
        {
            var url = urlGenerator.ContentUI(@event.AppId, @event.SchemaId, @event.ContentId);

            publishRequest.Properties["SquidexUrl"] = url;
        }

        publishRequest.TemplateCode = historyEvent.EventType;

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

    private static void SetUser(SquidexEvent @event, PublishDto publishRequest)
    {
        if (@event.Actor.IsUser)
        {
            publishRequest.CreatorId = @event.Actor.Identifier;
        }
    }

    private static string BuildTopic(string prefix, HistoryEvent @event)
    {
        return $"{prefix}/{@event.Channel.Replace('.', '/').Trim()}";
    }

    private static string GetAppPrefix(AppEvent appEvent)
    {
        return $"apps/{appEvent.AppId.Id}";
    }

    private static string GetTeamPrefix(TeamEvent teamEvent)
    {
        return $"apps/{teamEvent.TeamId}";
    }
}
