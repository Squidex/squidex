// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Core;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.Comments.DomainObject.Guards;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;

#pragma warning disable MA0022 // Return Task.FromResult instead of returning null

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject
{
    public sealed class CommentsGrain : GrainBase, ICommentsGrain
    {
        private readonly List<Envelope<CommentsEvent>> uncommittedEvents = new List<Envelope<CommentsEvent>>();
        private readonly List<Envelope<CommentsEvent>> events = new List<Envelope<CommentsEvent>>();
        private readonly IEventFormatter eventFormatter;
        private readonly IEventStore eventStore;
        private long version = EtagVersion.Empty;
        private string streamName;

        private long Version
        {
            get => version;
        }

        public CommentsGrain(IGrainIdentity identity,
            IEventFormatter eventFormatter,
            IEventStore eventStore)
            : base(identity)
        {
            this.eventFormatter = eventFormatter;
            this.eventStore = eventStore;
        }

        public override async Task OnActivateAsync()
        {
            streamName = $"comments-{Key}";

            var storedEvents = await eventStore.QueryReverseAsync(streamName, 100);

            foreach (var @event in storedEvents)
            {
                var parsedEvent = eventFormatter.Parse(@event);

                version = @event.EventStreamNumber;

                events.Add(parsedEvent.To<CommentsEvent>());
            }
        }

        public Task<CommandResult> ExecuteAsync(CommentsCommand command)
        {
            switch (command)
            {
                case CreateComment createComment:
                    return Upsert(createComment, c =>
                    {
                        GuardComments.CanCreate(c);

                        Create(c);
                    });

                case UpdateComment updateComment:
                    return Upsert(updateComment, c =>
                    {
                        GuardComments.CanUpdate(c, Key.ToString(), events);

                        Update(c);
                    });

                case DeleteComment deleteComment:
                    return Upsert(deleteComment, c =>
                    {
                        GuardComments.CanDelete(c, Key.ToString(), events);

                        Delete(c);
                    });

                default:
                    ThrowHelper.NotSupportedException();
                    return default!;
            }
        }

        private async Task<CommandResult> Upsert<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : CommentsCommand
        {
            Guard.NotNull(command);
            Guard.NotNull(handler);

            if (command.ExpectedVersion > EtagVersion.Any && command.ExpectedVersion != Version)
            {
                throw new DomainObjectVersionException(Key.ToString(), Version, command.ExpectedVersion);
            }

            var previousVersion = version;

            try
            {
                handler(command);

                if (uncommittedEvents.Count > 0)
                {
                    var commitId = Guid.NewGuid();

                    var eventData = uncommittedEvents.Select(x => eventFormatter.ToEventData(x, commitId)).ToList();

                    await eventStore.AppendAsync(commitId, streamName, previousVersion, eventData);
                }

                events.AddRange(uncommittedEvents);

                return CommandResult.Empty(Key, Version, previousVersion);
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

        public List<Envelope<CommentsEvent>> GetUncommittedEvents()
        {
            return uncommittedEvents;
        }

        public Task<CommentsResult> GetCommentsAsync(long sinceVersion = EtagVersion.Any)
        {
            return Task.FromResult(CommentsResult.FromEvents(events, Version, (int)sinceVersion));
        }
    }
}
