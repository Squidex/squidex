// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public class ContentState : DomainObjectState<ContentState>, IContentEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public NamedId<Guid> SchemaId { get; set; }

        [DataMember]
        public NamedContentData Data { get; set; }

        [DataMember]
        public NamedContentData DataDraft { get; set; }

        [DataMember]
        public ScheduleJob ScheduleJob { get; set; }

        [DataMember]
        public bool IsPending { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public Status Status { get; set; }

        protected void On(ContentCreated @event)
        {
            SimpleMapper.Map(@event, this);

            DataDraft = @event.Data;
        }

        protected void On(ContentUpdated @event)
        {
            DataDraft = @event.Data;

            if (Data != null)
            {
                Data = @event.Data;
            }
        }

        protected void On(ContentUpdateProposed @event)
        {
            DataDraft = @event.Data;

            IsPending = true;
        }

        protected void On(ContentChangesDiscarded @event)
        {
            DataDraft = Data;

            IsPending = false;
        }

        protected void On(ContentChangesPublished @event)
        {
            ScheduleJob = null;

            Data = DataDraft;

            IsPending = false;
        }

        protected void On(ContentStatusChanged @event)
        {
            ScheduleJob = null;
            Status = @event.Status;

            if (@event.Status == Status.Published)
            {
                Data = DataDraft;
            }

            IsPending = false;
        }

        protected void On(ContentSchedulingCancelled @event)
        {
            ScheduleJob = null;
        }

        protected void On(ContentStatusScheduled @event)
        {
            ScheduleJob = new ScheduleJob(Guid.NewGuid(), @event.Status, @event.Actor, @event.DueTime);
        }

        protected void On(ContentDeleted @event)
        {
            IsDeleted = true;
        }

        public ContentState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
