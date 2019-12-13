﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Comments.Guards;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Comments
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
            get { return version; }
        }

        public CommentsGrain(IEventStore eventStore, IEventDataFormatter eventDataFormatter)
        {
            Guard.NotNull(eventStore);
            Guard.NotNull(eventDataFormatter);

            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
        }

        protected override async Task OnActivateAsync(string key)
        {
            streamName = $"comments-{key}";

            var storedEvents = await eventStore.QueryLatestAsync(streamName, 100);

            foreach (var @event in storedEvents)
            {
                var parsedEvent = eventDataFormatter.Parse(@event.Data);

                version = @event.EventStreamNumber;

                events.Add(parsedEvent.To<CommentsEvent>());
            }
        }

        public async Task<J<object>> ExecuteAsync(J<CommentsCommand> command)
        {
            var result = await ExecuteAsync(command.Value);

            return result.AsJ();
        }

        private Task<object> ExecuteAsync(CommentsCommand command)
        {
            switch (command)
            {
                case CreateComment createComment:
                    return Upsert(createComment, c =>
                    {
                        GuardComments.CanCreate(c);

                        Create(c);

                        return EntityCreatedResult.Create(createComment.CommentId, Version);
                    });

                case UpdateComment updateComment:
                    return Upsert(updateComment, c =>
                    {
                        GuardComments.CanUpdate(Key, events, c);

                        Update(c);

                        return new EntitySavedResult(Version);
                    });

                case DeleteComment deleteComment:
                    return Upsert(deleteComment, c =>
                    {
                        GuardComments.CanDelete(Key, events, c);

                        Delete(c);

                        return new EntitySavedResult(Version);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<object> Upsert<TCommand>(TCommand command, Func<TCommand, object> handler) where TCommand : CommentsCommand
        {
            Guard.NotNull(command);
            Guard.NotNull(handler);

            if (command.ExpectedVersion > EtagVersion.Any && command.ExpectedVersion != Version)
            {
                throw new DomainObjectVersionException(Key, GetType(), Version, command.ExpectedVersion);
            }

            var prevVersion = version;

            try
            {
                var result = handler(command);

                if (uncommittedEvents.Count > 0)
                {
                    var commitId = Guid.NewGuid();

                    var eventData = uncommittedEvents.Select(x => eventDataFormatter.ToEventData(x, commitId)).ToList();

                    await eventStore.AppendAsync(commitId, streamName, prevVersion, eventData);
                }

                events.AddRange(uncommittedEvents);

                return result;
            }
            catch
            {
                version = prevVersion;

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

        public Task<CommentsResult> GetCommentsAsync(long version = EtagVersion.Any)
        {
            return Task.FromResult(CommentsResult.FromEvents(events, Version, (int)version));
        }
    }
}
