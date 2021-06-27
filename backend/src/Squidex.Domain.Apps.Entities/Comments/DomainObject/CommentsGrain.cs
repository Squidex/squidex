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
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.Comments.DomainObject.Guards;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject
{
    public sealed class CommentsGrain : GrainOfString, ICommentsGrain
    {
        private readonly List<Envelope<CommentsEvent>> uncommittedEvents = new List<Envelope<CommentsEvent>>();
        private readonly List<Envelope<CommentsEvent>> events = new List<Envelope<CommentsEvent>>();
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private long version = EtagVersion.Empty;
        private string streamName;

        private long Version
        {
            get => version;
        }

        public CommentsGrain(IEventStore eventStore, IEventDataFormatter eventDataFormatter)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
        }

        protected override async Task OnActivateAsync(string key)
        {
            streamName = $"comments-{key}";

            var storedEvents = await eventStore.QueryLatestAsync(streamName, 100);

            foreach (var @event in storedEvents)
            {
                var parsedEvent = eventDataFormatter.Parse(@event);

                version = @event.EventStreamNumber;

                events.Add(parsedEvent.To<CommentsEvent>());
            }
        }

        public async Task<J<CommandResult>> ExecuteAsync(J<CommentsCommand> command)
        {
            var result = await ExecuteAsync(command.Value);

            return result.AsJ();
        }

        private Task<CommandResult> ExecuteAsync(CommentsCommand command)
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
                        GuardComments.CanUpdate(c, Key, events);

                        Update(c);
                    });

                case DeleteComment deleteComment:
                    return Upsert(deleteComment, c =>
                    {
                        GuardComments.CanDelete(c, Key, events);

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<CommandResult> Upsert<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : CommentsCommand
        {
            Guard.NotNull(command, nameof(command));
            Guard.NotNull(handler, nameof(handler));

            if (command.ExpectedVersion > EtagVersion.Any && command.ExpectedVersion != Version)
            {
                throw new DomainObjectVersionException(Key, Version, command.ExpectedVersion);
            }

            var previousVersion = version;

            try
            {
                handler(command);

                if (uncommittedEvents.Count > 0)
                {
                    var commitId = Guid.NewGuid();

                    var eventData = uncommittedEvents.Select(x => eventDataFormatter.ToEventData(x, commitId)).ToList();

                    await eventStore.AppendAsync(commitId, streamName, previousVersion, eventData);
                }

                events.AddRange(uncommittedEvents);

                return CommandResult.Empty(DomainId.Create(Key), Version, previousVersion);
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
