// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.Comments.DomainObject.Guards;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject;

public class CommentsStream : IAggregate
{
    private readonly List<Envelope<CommentsEvent>> uncommittedEvents = new List<Envelope<CommentsEvent>>();
    private readonly List<Envelope<CommentsEvent>> events = new List<Envelope<CommentsEvent>>();
    private readonly DomainId key;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly string streamName;
    private long version = EtagVersion.Empty;

    private long Version => version;

    public CommentsStream(
        DomainId key,
        IEventFormatter eventFormatter,
        IEventStore eventStore)
    {
        this.key = key;
        this.eventFormatter = eventFormatter;
        this.eventStore = eventStore;

        streamName = $"comments-{key}";
    }

    public virtual async Task LoadAsync(
        CancellationToken ct)
    {
        var storedEvents = await eventStore.QueryReverseAsync(streamName, 100, ct);

        foreach (var @event in storedEvents)
        {
            var parsedEvent = eventFormatter.Parse(@event);

            version = @event.EventStreamNumber;

            events.Add(parsedEvent.To<CommentsEvent>());
        }
    }

    public virtual async Task<CommandResult> ExecuteAsync(IAggregateCommand command,
        CancellationToken ct)
    {
        await LoadAsync(ct);

        switch (command)
        {
            case CreateComment createComment:
                return await Upsert(createComment, c =>
                {
                    GuardComments.CanCreate(c);

                    Create(c);
                }, ct);

            case UpdateComment updateComment:
                return await Upsert(updateComment, c =>
                {
                    GuardComments.CanUpdate(c, key.ToString(), events);

                    Update(c);
                }, ct);

            case DeleteComment deleteComment:
                return await Upsert(deleteComment, c =>
                {
                    GuardComments.CanDelete(c, key.ToString(), events);

                    Delete(c);
                }, ct);

            default:
                ThrowHelper.NotSupportedException();
                return null!;
        }
    }

    private async Task<CommandResult> Upsert<TCommand>(TCommand command, Action<TCommand> handler,
        CancellationToken ct) where TCommand : CommentsCommand
    {
        Guard.NotNull(command);
        Guard.NotNull(handler);

        if (command.ExpectedVersion > EtagVersion.Any && command.ExpectedVersion != Version)
        {
            throw new DomainObjectVersionException(key.ToString(), Version, command.ExpectedVersion);
        }

        var previousVersion = version;

        try
        {
            handler(command);

            if (uncommittedEvents.Count > 0)
            {
                var commitId = Guid.NewGuid();

                var eventData = uncommittedEvents.Select(x => eventFormatter.ToEventData(x, commitId)).ToList();

                await eventStore.AppendAsync(commitId, streamName, previousVersion, eventData, ct);
            }

            events.AddRange(uncommittedEvents);

            return CommandResult.Empty(key, Version, previousVersion);
        }
        catch
        {
            version = previousVersion;

            throw;
        }
        finally
        {
            uncommittedEvents.Clear();
        }
    }

    public void Create(CreateComment command)
    {
        RaiseEvent(SimpleMapper.Map(command, new CommentCreated()));
    }

    public void Update(UpdateComment command)
    {
        RaiseEvent(SimpleMapper.Map(command, new CommentUpdated()));
    }

    public void Delete(DeleteComment command)
    {
        RaiseEvent(SimpleMapper.Map(command, new CommentDeleted()));
    }

    private void RaiseEvent(CommentsEvent @event)
    {
        uncommittedEvents.Add(Envelope.Create(@event));

        version++;
    }

    public virtual List<Envelope<CommentsEvent>> GetUncommittedEvents()
    {
        return uncommittedEvents;
    }

    public virtual CommentsResult GetComments(long sinceVersion = EtagVersion.Any)
    {
        return CommentsResult.FromEvents(events, Version, (int)sinceVersion);
    }
}
