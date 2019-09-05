// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

#pragma warning disable IDE0060 // Remove unused parameter

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

        public void ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case ContentCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        UpdateData(null, e.Data, false);

                        break;
                    }

                case ContentChangesPublished _:
                    {
                        ScheduleJob = null;

                        UpdateData(DataDraft, null, false);

                        break;
                    }

                case ContentStatusChanged e:
                    {
                        ScheduleJob = null;

                        SimpleMapper.Map(e, this);

                        if (e.Status == Status.Published)
                        {
                            UpdateData(DataDraft, null, false);
                        }

                        break;
                    }

                case ContentUpdated e:
                    {
                        UpdateData(e.Data, e.Data, false);

                        break;
                    }

                case ContentUpdateProposed e:
                    {
                        UpdateData(null, e.Data, true);

                        break;
                    }

                case ContentChangesDiscarded _:
                    {
                        UpdateData(null, Data, false);

                        break;
                    }

                case ContentSchedulingCancelled _:
                    {
                        ScheduleJob = null;

                        break;
                    }

                case ContentStatusScheduled e:
                    {
                        ScheduleJob = ScheduleJob.Build(e.Status, e.Actor, e.DueTime);

                        break;
                    }

                case ContentDeleted _:
                    {
                        IsDeleted = true;

                        break;
                    }
            }
        }

        public override ContentState Apply(Envelope<IEvent> @event)
        {
            return Clone().Update(@event, (e, s) => s.ApplyEvent(e));
        }

        private void UpdateData(NamedContentData data, NamedContentData dataDraft, bool isPending)
        {
            if (data != null)
            {
                Data = data;
            }

            if (dataDraft != null)
            {
                DataDraft = dataDraft;
            }

            IsPending = isPending;
        }
    }
}
