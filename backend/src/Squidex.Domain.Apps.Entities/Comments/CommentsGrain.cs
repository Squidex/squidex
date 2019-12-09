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
using Squidex.Domain.Apps.Entities.Comments.Guards;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsGrain : GrainOfString, ICommentsGrain
    {
        private readonly IStore<string> store;
        private readonly List<Envelope<CommentsEvent>> uncommittedEvents = new List<Envelope<CommentsEvent>>();
        private readonly List<Envelope<CommentsEvent>> events = new List<Envelope<CommentsEvent>>();
        private IPersistence persistence;

        private long Version
        {
            get { return events.Count + uncommittedEvents.Count - 1; }
        }

        public CommentsGrain(IStore<string> store)
        {
            Guard.NotNull(store);

            this.store = store;
        }

        protected override Task OnActivateAsync(string key)
        {
            persistence = store.WithEventSourcing(GetType(), key, ApplyEvent);

            return persistence.ReadAsync();
        }

        private void ApplyEvent(Envelope<IEvent> @event)
        {
            events.Add(@event.To<CommentsEvent>());
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
                        GuardComments.CanUpdate(events, c);

                        Update(c);

                        return new EntitySavedResult(Version);
                    });

                case DeleteComment deleteComment:
                    return Upsert(deleteComment, c =>
                    {
                        GuardComments.CanDelete(events, c);

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

            try
            {
                var result = handler(command);

                if (uncommittedEvents.Count > 0)
                {
                    await persistence.WriteEventsAsync(uncommittedEvents.Select(x => x.To<IEvent>()));
                }

                events.AddRange(uncommittedEvents);

                return result;
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
        }

        public Task<CommentsResult> GetCommentsAsync(long version = EtagVersion.Any)
        {
            return Task.FromResult(CommentsResult.FromEvents(events, Version, (int)version));
        }
    }
}
