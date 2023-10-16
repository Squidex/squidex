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
    private readonly IEventStore eventStore;
    private readonly IEventFormatter eventFormatter;
    private readonly IUserResolver userResolver;
    private readonly IClock clock;
    private IDocumentManager? currentManager;


    public NotificationCreator(
        IEventStore eventStore,
        IEventFormatter eventFormatter,
        IUserResolver userResolver,
        IClock clock)
    {
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

    public async Task NotifyAsync(string userId, string text, Uri? url,
        CancellationToken ct = default)
    {
        if (currentManager == null)
        {
            return;
        }

        var notificationsContext = new DocumentContext($"notifications/{userId}", 0);

        await currentManager.UpdateDocAsync(notificationsContext, (doc) =>
        {
            var stream = doc.Array("stream");

            using (var transaction = doc.WriteTransaction())
            {
                var comment = new Comment(DomainId.NewGuid(), clock.GetCurrentInstant(), RefToken.User(userId), text, url);

                stream.InsertRange(transaction, stream.Length, comment.ToInput());
            }
        }, ct);
    }

    public ValueTask OnDocumentLoadedAsync(DocumentLoadEvent @event)
    {
        if (!@event.Context.DocumentName.StartsWith("chat/", StringComparison.OrdinalIgnoreCase))
        {
            return default;
        }

        var stream = @event.Document.Map("stream");

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
                List<Comment>? comments = null;

                await @event.Source.UpdateDocAsync(@event.Context, (doc) =>
                {
                    using (var transaction = @event.Document.ReadTransaction())
                    {
                        comments = newComments.Select(x => x.To<Comment>(transaction)).ToList();
                    }
                });

                if (comments == null)
                {
                    return;
                }

                var commentsId = DomainId.Create(@event.Context.DocumentName);

                foreach (var comment in comments)
                {
                    var eventPayload = await CreateEventAsync(comment, commentsId);
                    var eventEnvelope = Envelope.Create<IEvent>(eventPayload);
                    var eventData = eventFormatter.ToEventData(eventEnvelope, Guid.NewGuid());

                    await eventStore.AppendAsync(Guid.NewGuid(), commentsId.ToString(), EtagVersion.Any, new List<EventData> { eventData });

                    foreach (var mentionedUser in eventPayload.Mentions.OrEmpty())
                    {
                        await NotifyAsync(mentionedUser, comment.Text, comment.Url);
                    }
                }
            }).Forget();
        });

        return default;
    }

    private async Task<CommentCreated> CreateEventAsync(Comment comment, DomainId commentsId)
    {
        var @event = new CommentCreated
        {
            Actor = comment.User,
            CommentId = comment.Id,
            CommentsId = commentsId
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

    [GeneratedRegex(@"@(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*", RegexOptions.Compiled | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex BuildMentionRegex();
}
