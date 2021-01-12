﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed partial class ContentDomainObject
    {
        public sealed class State : DomainObjectState<State>, IContentEntity
        {
            public NamedId<DomainId> AppId { get; set; }

            public NamedId<DomainId> SchemaId { get; set; }

            public ContentVersion? NewVersion { get; set; }

            public ContentVersion CurrentVersion { get; set; }

            public ScheduleJob? ScheduleJob { get; set; }

            [IgnoreDataMember]
            public DomainId UniqueId
            {
                get { return DomainId.Combine(AppId, Id); }
            }

            [IgnoreDataMember]
            public NamedContentData Data
            {
                get { return NewVersion?.Data ?? CurrentVersion.Data; }
            }

            [IgnoreDataMember]
            public Status EditingStatus
            {
                get { return NewStatus ?? Status; }
            }

            [IgnoreDataMember]
            public Status Status
            {
                get { return CurrentVersion.Status; }
            }

            [IgnoreDataMember]
            public Status? NewStatus
            {
                get { return NewVersion?.Status; }
            }

            public override bool ApplyEvent(IEvent @event, EnvelopeHeaders headers)
            {
                switch (@event)
                {
                    case ContentCreated e:
                        {
                            Id = e.ContentId;

                            SimpleMapper.Map(e, this);

                            CurrentVersion = new ContentVersion(e.Status, e.Data);

                            break;
                        }

                    case ContentDraftCreated e:
                        {
                            NewVersion = new ContentVersion(e.Status, e.MigratedData ?? CurrentVersion.Data);

                            ScheduleJob = null;

                            break;
                        }

                    case ContentDraftDeleted _:
                        {
                            NewVersion = null;

                            ScheduleJob = null;

                            break;
                        }

                    case ContentStatusChanged e:
                        {
                            ScheduleJob = null;

                            if (NewVersion != null)
                            {
                                if (e.Status == Status.Published)
                                {
                                    CurrentVersion = new ContentVersion(e.Status, NewVersion.Data);

                                    NewVersion = null;
                                }
                                else
                                {
                                    NewVersion = NewVersion.WithStatus(e.Status);
                                }
                            }
                            else
                            {
                                CurrentVersion = CurrentVersion.WithStatus(e.Status);
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
                            if (NewVersion != null)
                            {
                                NewVersion = NewVersion.WithData(e.Data);
                            }
                            else
                            {
                                CurrentVersion = CurrentVersion.WithData(e.Data);
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
}
