// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.Comments.Guards;
using Squidex.Domain.Apps.Entities.Comments.State;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsGrain : DomainObjectGrainBase<CommentsState>, ICommentGrain
    {
        private readonly IStore<Guid> store;
        private readonly List<Envelope<CommentsEvent>> events = new List<Envelope<CommentsEvent>>();
        private CommentsState snapshot = new CommentsState { Version = EtagVersion.Empty };
        private IPersistence persistence;

        public override CommentsState Snapshot
        {
            get { return snapshot; }
        }

        public CommentsGrain(IStore<Guid> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        protected override void ApplyEvent(Envelope<IEvent> @event)
        {
            snapshot = new CommentsState { Version = snapshot.Version + 1 };

            events.Add(@event.To<CommentsEvent>());
        }

        protected override void RestorePreviousSnapshot(CommentsState previousSnapshot, long previousVersion)
        {
            snapshot = previousSnapshot;
        }

        protected override Task ReadAsync(Type type, Guid id)
        {
            persistence = store.WithEventSourcing(GetType(), id, ApplyEvent);

            return persistence.ReadAsync();
        }

        protected override async Task WriteAsync(Envelope<IEvent>[] events, long previousVersion)
        {
            if (events.Length > 0)
            {
                await persistence.WriteEventsAsync(events);
            }
        }

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateComment createComment:
                    return UpsertReturn(createComment, c =>
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
                    });

                case DeleteComment deleteComment:
                    return Upsert(deleteComment, c =>
                    {
                        GuardComments.CanDelete(events, c);

                        Delete(c);
                    });

                default:
                    throw new NotSupportedException();
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

        public Task<CommentsResult> GetCommentsAsync(long version = EtagVersion.Any)
        {
            return Task.FromResult(CommentsResult.FromEvents(events, Version, (int)version));
        }
    }
}
