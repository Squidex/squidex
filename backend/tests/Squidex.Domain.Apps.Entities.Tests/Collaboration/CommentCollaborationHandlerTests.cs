﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using LoremNET;
using NodaTime;
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;
using YDotNet.Document;
using YDotNet.Document.Cells;
using YDotNet.Extensions;
using YDotNet.Server;

namespace Squidex.Domain.Apps.Entities.Collaboration;

public class CommentCollaborationHandlerTests : GivenContext
{
    private readonly IClock clock = A.Fake<IClock>();
    private readonly IEventFormatter eventFormatter = A.Fake<IEventFormatter>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly IDocumentManager documentManager = A.Fake<IDocumentManager>();
    private readonly CommentCollaborationHandler sut;

    public CommentCollaborationHandlerTests()
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>._, default))
            .Returns(Task.FromResult<IUser?>(null));

        A.CallTo(() => documentManager.UpdateDocAsync(A<DocumentContext>._, A<Action<Doc>>._, A<CancellationToken>._))
            .Invokes(c =>
            {
                c.GetArgument<Action<Doc>>(1)!(null!);
            });

        sut = new CommentCollaborationHandler(TestUtils.DefaultSerializer, eventStore, eventFormatter, userResolver, clock);
    }

    [Fact]
    public void Should_provider_user_document_name()
    {
        var name = sut.UserDocument("user42");

        Assert.Equal("users/user42", name);
    }

    [Fact]
    public void Should_provider_app_document_name()
    {
        var name = sut.ResourceDocument(AppId, DomainId.Create("resource42"));

        Assert.Equal($"apps/{AppId}/resource42", name);
    }

    [Fact]
    public async Task Should_create_comment()
    {
        await sut.OnInitializedAsync(documentManager);

        var commentsId = DomainId.Create("resource42");

        var document = new Doc();
        var docName = sut.ResourceDocument(AppId, commentsId);

        Output? addedInput = null;
        document.Array("stream").ObserveDeep(events =>
        {
            addedInput = events.Single().ArrayEvent.Delta.Single().Values.Single();
        });

        A.CallTo(() => documentManager.UpdateDocAsync(
                A<DocumentContext>.That.Matches(x => x.DocumentName == docName),
                A<Action<Doc>>._,
                A<CancellationToken>._))
            .Invokes(c =>
            {
                c.GetArgument<Action<Doc>>(1)!(document);
            });

        await sut.CommentAsync(AppId, commentsId, "My Comment", User, null, true, default);

        var commentJson = addedInput!.ToJson(document);
        var commentItem = TestUtils.DefaultSerializer.Deserialize<Comment>(commentJson);

        commentItem.Should().BeEquivalentTo(
            new Comment(clock.GetCurrentInstant(), User, "My Comment", null, true));
    }

    [Fact]
    public async Task Should_create_notification()
    {
        await sut.OnInitializedAsync(documentManager);

        var document = new Doc();
        var docName = sut.UserDocument(User.Identifier);

        Output? addedInput = null;
        document.Array("stream").ObserveDeep(events =>
        {
            addedInput = events.Single().ArrayEvent.Delta.Single().Values.Single();
        });

        A.CallTo(() => documentManager.UpdateDocAsync(
                A<DocumentContext>.That.Matches(x => x.DocumentName == docName),
                A<Action<Doc>>._,
                A<CancellationToken>._))
            .Invokes(c =>
            {
                c.GetArgument<Action<Doc>>(1)!(document);
            });

        await sut.NotifyAsync(User.Identifier, "My Notification", User, null, true, default);

        var commentJson = addedInput!.ToJson(document);
        var commentItem = TestUtils.DefaultSerializer.Deserialize<Comment>(commentJson);

        commentItem.Should().BeEquivalentTo(
            new Comment(clock.GetCurrentInstant(), User, "My Notification", null, true));
    }

    [Fact]
    public async Task Should_publish_event_for_comment()
    {
        var text = "My Comment";

        var commentsId = DomainId.Create("resource42");
        var commentItem = new Comment(clock.GetCurrentInstant(), User, text);

        var storedEvent = await CreateCommentAsync(commentsId, commentItem);

        storedEvent?.Payload.Should().BeEquivalentTo(
            new CommentCreated
            {
                Actor = User,
                AppId = AppId,
                CommentId = default,
                CommentsId = commentsId,
                Text = commentItem.Text,
                Mentions = null,
            }, opts => opts.Excluding(x => x.CommentId));
    }

    [Fact]
    public async Task Should_not_enrich_comment_with_mentioned_users_if_users_not_found()
    {
        var text = "Hi @mail1@squidex.io, @mail2@squidex.io and @notfound@squidex.io";

        var commentsId = DomainId.Create("resource42");
        var commentItem = new Comment(clock.GetCurrentInstant(), User, text);

        var storedEvent = await CreateCommentAsync(commentsId, commentItem);

        storedEvent?.Payload.Should().BeEquivalentTo(
            new CommentCreated
            {
                Actor = User,
                AppId = AppId,
                CommentId = default,
                CommentsId = commentsId,
                Text = commentItem.Text,
                Mentions = null,
            }, opts => opts.Excluding(x => x.CommentId));
    }

    [Fact]
    public async Task Should_enrich_comment_with_mentioned_users()
    {
        SetupUser("id1", "mail1@squidex.io");
        SetupUser("id2", "mail2@squidex.io");

        var text = "Hi @mail1@squidex.io, @mail2@squidex.io and @notfound@squidex.io";

        var commentsId = DomainId.Create("resource42");
        var commentItem = new Comment(clock.GetCurrentInstant(), User, text);

        var storedEvent = await CreateCommentAsync(commentsId, commentItem);

        storedEvent?.Payload.Should().BeEquivalentTo(
            new CommentCreated
            {
                Actor = User,
                AppId = AppId,
                CommentId = default,
                CommentsId = commentsId,
                Text = commentItem.Text,
                Mentions = new string[] { "id1", "id2" }
            }, opts => opts.Excluding(x => x.CommentId));
    }

    [Fact]
    public async Task Should_enrich_comment_with_mentioned_users_and_long_text()
    {
        SetupUser("id1", "mail1@squidex.io");
        SetupUser("id2", "mail2@squidex.io");

        var text = $"Hi @mail1@squidex.io, @mail2@squidex.io and @notfound@squidex.io {Lorem.Paragraph(200, 10)}";

        var commentsId = DomainId.Create("resource42");
        var commentItem = new Comment(clock.GetCurrentInstant(), User, text);

        var storedEvent = await CreateCommentAsync(commentsId, commentItem);

        storedEvent?.Payload.Should().BeEquivalentTo(
            new CommentCreated
            {
                Actor = User,
                AppId = AppId,
                CommentId = default,
                CommentsId = commentsId,
                Text = commentItem.Text,
                Mentions = new string[] { "id1", "id2" }
            }, opts => opts.Excluding(x => x.CommentId));
    }

    private async Task<Envelope<IEvent>?> CreateCommentAsync(DomainId commentsId, Comment comment)
    {
        var document = new Doc();
        var docName = sut.ResourceDocument(AppId, commentsId);

        var stream = document.Array("stream");

        await sut.OnDocumentLoadedAsync(new DocumentLoadEvent
        {
            Context = new DocumentContext(docName, 0),
            Document = document,
            Source = documentManager,
        });

        var commentJson = TestUtils.DefaultSerializer.Serialize(comment);

        Envelope<IEvent>? storedEvent = null;

        A.CallTo(() => eventFormatter.ToEventData(A<Envelope<IEvent>>._, A<Guid>._, true))
            .Invokes(c =>
            {
                storedEvent = c.GetArgument<Envelope<IEvent>>(0);
            });

        using (var transaction = document.WriteTransaction())
        {
            stream.InsertRange(transaction, 0, InputFactory.FromJson(commentJson));
        }

        await WaitForCompletion();

        var streamName = $"comments-{DomainId.Combine(AppId.Id, commentsId)}";

        A.CallTo(() => eventStore.AppendAsync(A<Guid>._, streamName, EtagVersion.Any, A<ICollection<EventData>>._, A<CancellationToken>._))
            .MustHaveHappened();

        return storedEvent;
    }

    private void SetupUser(string id, string email)
    {
        var user = UserMocks.User(id, email);

        A.CallTo(() => userResolver.FindByIdOrEmailAsync(email, default))
            .Returns(user);
    }

    private async Task WaitForCompletion()
    {
        using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(5000));

        while (sut.HasPendingJobs)
        {
            tcs.Token.ThrowIfCancellationRequested();

            await Task.Delay(20, tcs.Token);
        }
    }
}
