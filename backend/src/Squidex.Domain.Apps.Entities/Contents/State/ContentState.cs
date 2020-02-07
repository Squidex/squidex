// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Contents.State
{
    public sealed class ContentState : DomainObjectState<ContentState>, IContentEntity, IContentInfo
    {
        public NamedId<Guid> AppId { get; set; }

        public NamedId<Guid> SchemaId { get; set; }

        public Status? NewStatus { get; set; }

        public Status Status { get; set; }

        public ScheduleJob? ScheduleJob { get; set; }

        public NamedContentData? NewData { get; set; }

        public NamedContentData Data { get; set; }

        public NamedContentData EditingData
        {
            get { return NewData ?? Data; }
        }

        public Status EditingStatus
        {
            get { return NewStatus ?? Status; }
        }

        public override bool ApplyEvent(IEvent @event, EnvelopeHeaders headers)
        {
            switch (@event)
            {
                case ContentCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        break;
                    }

                case ContentDraftCreated e:
                    {
                        NewData = Data;
                        NewStatus = e.Status;

                        ScheduleJob = null;

                        break;
                    }

                case ContentDraftDeleted _:
                    {
                        NewData = null;
                        NewStatus = null;

                        ScheduleJob = null;

                        break;
                    }

                case ContentStatusChanged e:
                    {
                        if (NewStatus.HasValue)
                        {
                            if (e.Status == Status.Published)
                            {
                                Status = e.Status;

                                Data = NewData!;

                                NewStatus = null;
                                NewData = null;
                            }
                            else
                            {
                                NewStatus = e.Status;
                            }
                        }
                        else
                        {
                            Status = e.Status;
                        }

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

                case ContentUpdated e:
                    {
                        if (NewStatus.HasValue)
                        {
                            NewData = e.Data;
                        }
                        else
                        {
                            Data = e.Data;
                        }

                        break;
                    }

                case ContentDeleted _:
                    {
                        IsDeleted = true;

                        break;
                    }
            }

            return true;
        }
    }
}
