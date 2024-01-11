// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NodaTime;
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

namespace Squidex.Domain.Apps.Entities.Collaboration;

public sealed partial class CommentCollaborationHandler : IDocumentCallback, ICollaborationService
{
    private static readonly Regex MentionRegex = BuildMentionRegex();
    private readonly IJsonSerializer jsonSerializer;
    private readonly IEventStore eventStore;
    private readonly IEventFormatter eventFormatter;
    private readonly IUserResolver userResolver;
    private readonly IClock clock;
    private readonly ILogger<CommentCollaborationHandler> log;
    private IDocumentManager? currentManager;

    public Task LastTask { get; private set; }

    public CommentCollaborationHandler(
        IJsonSerializer jsonSerializer,
        IEventStore eventStore,
        IEventFormatter eventFormatter,
        IUserResolver userResolver,
        IClock clock,
        ILogger<CommentCollaborationHandler> log)
    {
        this.jsonSerializer = jsonSerializer;
        this.eventStore = eventStore;
        this.eventFormatter = eventFormatter;
        this.userResolver = userResolver;
        this.clock = clock;
        this.log = log;
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

        // Use the update method to ensure that only one thread has access to the doc.
        await currentManager.UpdateDocAsync(notificationsContext, doc =>
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
        if (!IsResourceOrUserDocument(@event.Context.DocumentName, out var appId, out var resourceId))
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
                // Just store the last task for tests.
                LastTask = Task.CompletedTask;
                return;
            }

            LastTask = Task.Run(async () =>
            {
                try
                {
                    // Run in an extra task to prevent deadlocks with the outer transaction.
                    await HandleAsync(@event, appId, resourceId, newComments);
                }
                catch (Exception ex)
                {
                    // We are in an extra task, so the exception would be probably swallowed.
                    log.LogError(ex, "Failed to handle yjs event.");
                    throw;
                }
            });
        });

        return default;
    }

    private async Task HandleAsync(DocumentLoadEvent @event, NamedId<DomainId> appId, DomainId resourceId, Output[] newComments)
    {
        var comments = new List<Comment>();

        // Use the update method to ensure that only one thread has access to the doc.
        await @event.Source.UpdateDocAsync(@event.Context, (doc) =>
        {
            using (var transaction = @event.Document.ReadTransaction())
            {
                foreach (var output in newComments)
                {
                    // Just use the json string for debuggability.
                    var json = output.ToJson(transaction);

                    var comment = jsonSerializer.Deserialize<Comment>(json);

                    if (!comment.SkipHandlers)
                    {
                        comments.Add(comment);
                    }
                }
            }
        });

        var streamName = $"comments-{DomainId.Combine(appId, resourceId)}";

        foreach (var comment in comments)
        {
            var commentEvent = await CreateEventAsync(comment, appId, resourceId);

            var eventBody = Envelope.Create<IEvent>(commentEvent);
            var eventData = eventFormatter.ToEventData(eventBody, Guid.NewGuid());

            await eventStore.AppendAsync(Guid.NewGuid(), streamName, EtagVersion.Any, new List<EventData> { eventData });

            foreach (var mentionedUser in commentEvent.Mentions.OrEmpty())
            {
                await NotifyAsync(mentionedUser, comment.Text, RefToken.User(mentionedUser), comment.Url, true);
            }
        }
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
        return $"users/{userId}";
    }

    public string ResourceDocument(NamedId<DomainId> appId, DomainId resourceId)
    {
        return $"apps/{appId}/{resourceId}";
    }

    private static bool IsResourceOrUserDocument(string name, out NamedId<DomainId> appId, out DomainId resourceId)
    {
        resourceId = default;

        // Result can be null, if the method returns null.
        appId = default!;

        static bool IsResourceDocument(string name, ref NamedId<DomainId> appId, ref DomainId resourceId)
        {
            var parts = name.Split('/');

            if (parts.Length < 3 || !NamedId<DomainId>.TryParse(parts[1], DomainId.TryParse, out appId!))
            {
                return false;
            }

            // Assume tha tour ID could also have slashes.
            resourceId = DomainId.Create(string.Join('/', parts.Skip(2)));
            return true;
        }

        static bool IsUserDocument(string name, ref NamedId<DomainId> appId, ref DomainId resourceId)
        {
            var parts = name.Split('/');

            if (parts.Length < 2)
            {
                return false;
            }

            // Use a dummy value for the app ID.
            appId = CommentCreated.NoApp;

            // Assume tha tour ID could also have slashes.
            resourceId = DomainId.Create(string.Join('/', parts.Skip(1)));
            return true;
        }

        if (name.StartsWith("apps/", StringComparison.Ordinal))
        {
            return IsResourceDocument(name, ref appId, ref resourceId);
        }

        if (name.StartsWith("users/", StringComparison.Ordinal))
        {
            return IsUserDocument(name, ref appId, ref resourceId);
        }

        return false;
    }

    [GeneratedRegex(@"@(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*", RegexOptions.Compiled | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex BuildMentionRegex();
}
