// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;
using YDotNet.Document.Cells;
using YDotNet.Document.Types.Events;
using YDotNet.Extensions;
using YDotNet.Server;

namespace Squidex.Domain.Apps.Entities.Comments;

public sealed partial class NotificationCreator : IDocumentCallback, INotificationPublisher
{
    private static readonly Regex MentionRegex = BuildMentionRegex();
    private readonly IJsonSerializer jsonSerializer;
    private readonly IEventStore eventStore;
    private readonly IEventFormatter eventFormatter;
    private readonly IUserResolver userResolver;
    private readonly IClock clock;
    private IDocumentManager? currentManager;

    public NotificationCreator(
        IJsonSerializer jsonSerializer,
        IEventStore eventStore,
        IEventFormatter eventFormatter,
        IUserResolver userResolver,
        IClock clock)
    {
        this.jsonSerializer = jsonSerializer;
        this.eventStore = eventStore;
        this.eventFormatter = eventFormatter;
        this.userResolver = userResolver;
        this.clock = clock;
    }

    public ValueTask OnInitializedAsync(IDocumentManager manager)
    {
        currentManager = manager;
        return default;
    }

    public Task NotifyAsync(string userId, string text, RefToken actor, Uri? url, bool skipHandlers,
        CancellationToken ct = default)
    {
        return CommentAsync(UserDocument(userId), text, actor, url, skipHandlers, ct);
    }

    public Task CommentAsync(NamedId<DomainId> appId, DomainId resourceId, string text, RefToken actor, Uri? url, bool skipHandlers,
        CancellationToken ct = default)
    {
        return CommentAsync(ResourceDocument(appId, resourceId), text, actor, url, skipHandlers, ct);
    }

    private async Task CommentAsync(string documentName, string text, RefToken actor, Uri? url, bool skipHandlers,
        CancellationToken ct)
    {
        if (currentManager == null)
        {
            return;
        }

        var notificationsContext = new DocumentContext(documentName, 0);

        await currentManager.UpdateDocAsync(notificationsContext, (doc) =>
        {
            var stream = doc.Array("stream");

            using (var transaction = doc.WriteTransaction())
            {
                var commentValue = new Comment(clock.GetCurrentInstant(), actor, text, url, skipHandlers);
                var commentJson = jsonSerializer.Serialize(commentValue);

                stream.InsertRange(transaction, stream.Length, InputFactory.FromJson(commentJson));
            }
        }, ct);
    }

    public ValueTask OnDocumentLoadedAsync(DocumentLoadEvent @event)
    {
        if (!IsResourceDocument(@event.Context.DocumentName, out var appId, out var resourceId))
        {
            return default;
        }

        var stream = @event.Document.Array("stream");

        stream.ObserveDeep(changes =>
        {
            var newComments =
                changes
                    .Where(x => x.Tag == EventBranchTag.Array)
                    .Select(x => x.ArrayEvent)
                    .SelectMany(x => x.Delta).Where(x => x.Tag == EventChangeTag.Add)
                    .SelectMany(x => x.Values).Where(x => x.Tag == OutputTag.JsonObject)
                    .ToArray();

            if (newComments.Length == 0)
            {
                return;
            }

            Task.Run(async () =>
            {
                var comments = new List<Comment>();

                await @event.Source.UpdateDocAsync(@event.Context, (doc) =>
                {
                    using (var transaction = @event.Document.ReadTransaction())
                    {
                        foreach (var output in newComments)
                        {
                            var json = output.ToJson(transaction);

                            var comment = jsonSerializer.Deserialize<Comment>(json);

                            if (!comment.SkipHandlers)
                            {
                                comments.Add(comment);
                            }
                        }
                    }
                });

                var commentsId = DomainId.Create(@event.Context.DocumentName);

                foreach (var comment in comments)
                {
                    var @event = await CreateEventAsync(comment, appId, commentsId);

                    var eventBody = Envelope.Create<IEvent>(@event);
                    var eventData = eventFormatter.ToEventData(eventBody, Guid.NewGuid());

                    await eventStore.AppendAsync(Guid.NewGuid(), commentsId.ToString(), EtagVersion.Any, new List<EventData> { eventData });

                    foreach (var mentionedUser in @event.Mentions.OrEmpty())
                    {
                        await NotifyAsync(mentionedUser, comment.Text, RefToken.User(mentionedUser), comment.Url, true);
                    }
                }
            }).Forget();
        });

        return default;
    }

    private async Task<CommentCreated> CreateEventAsync(Comment comment, NamedId<DomainId> appId, DomainId commentsId)
    {
        var @event = new CommentCreated
        {
            Actor = comment.User,
            CommentId = DomainId.NewGuid(),
            CommentsId = commentsId,
            AppId = appId,
        };

        SimpleMapper.Map(comment, @event);

        await MentionUsersAsync(@event);
        return @event;
    }

    private async Task MentionUsersAsync(CommentCreated comment)
    {
        if (string.IsNullOrWhiteSpace(comment.Text))
        {
            return;
        }

        var emails = MentionRegex.Matches(comment.Text).Select(x => x.Value[1..]).ToArray();

        if (emails.Length == 0)
        {
            return;
        }

        var mentions = new List<string>();

        foreach (var email in emails)
        {
            var user = await userResolver.FindByIdOrEmailAsync(email);

            if (user != null)
            {
                mentions.Add(user.Id);
            }
        }

        if (mentions.Count > 0)
        {
            comment.Mentions = mentions.ToArray();
        }
    }

    public string UserDocument(string userId)
    {
        return $"notifications/{userId}";
    }

    public string ResourceDocument(NamedId<DomainId> appId, DomainId resourceId)
    {
        return $"apps/{appId}/comments/{resourceId}";
    }

    private static bool IsResourceDocument(string name, out NamedId<DomainId> appId, out DomainId resourceId)
    {
        resourceId = default;

        if (!name.StartsWith("apps", StringComparison.Ordinal))
        {
            appId = default!;
            return false;
        }

        var parts = name.Split('/');

        if (parts.Length < 4 || !NamedId<DomainId>.TryParse(parts[1], DomainId.TryParse, out appId!))
        {
            appId = default!;
            return false;
        }

        resourceId = DomainId.Create(string.Join('/', parts.Skip(3)));
        return true;
    }

    [GeneratedRegex(@"@(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*", RegexOptions.Compiled | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex BuildMentionRegex();
}
