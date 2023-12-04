// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public partial class ContentDomainObject
{
    protected override WriteContent Apply(WriteContent snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        ContentData Data()
        {
            return snapshot.NewVersion.Data ?? snapshot.CurrentVersion.Data;
        }

        ContentData CurrentData()
        {
            return snapshot.CurrentVersion.Data;
        }

        switch (@event.Payload)
        {
            case ContentCreated e:
                newSnapshot = new WriteContent
                {
                    Id = e.ContentId,
                    AppId = e.AppId,
                    ScheduleJob = null,
                    SchemaId = e.SchemaId,
                    CurrentVersion = new ContentVersion(e.Status, e.Data)
                };

                break;

            case ContentDraftCreated e:
                newSnapshot = snapshot with
                {
                    NewVersion = new ContentVersion(e.Status, e.MigratedData?.UseSameFields(CurrentData()) ?? CurrentData()),
                    // Implictely cancels any pending update jobs.
                    ScheduleJob = null,
                };

                break;

            case ContentDraftDeleted:
                newSnapshot = snapshot with
                {
                    NewVersion = null,
                    // Implictely cancels any pending update jobs.
                    ScheduleJob = null,
                };

                break;

            case ContentStatusChanged e when snapshot.NewVersion != null && e.Status == Status.Published:
                newSnapshot = snapshot with
                {
                    CurrentVersion = new ContentVersion(e.Status, snapshot.NewVersion.Data.UseSameFields(CurrentData())),
                    // Discards the draft version.
                    NewVersion = null,
                    // Implictely cancels any pending update jobs.
                    ScheduleJob = null,
                };

                break;

            case ContentStatusChanged e when snapshot.NewVersion != null:
                newSnapshot = snapshot with
                {
                    NewVersion = snapshot.NewVersion with { Status = e.Status },
                    // Implictely cancels any pending update jobs.
                    ScheduleJob = null,
                };

                break;

            case ContentStatusChanged e:
                newSnapshot = snapshot with
                {
                    CurrentVersion = snapshot.CurrentVersion with { Status = e.Status },
                    // Implictely cancels any pending update jobs.
                    ScheduleJob = null,
                };

                break;

            case ContentUpdated e when snapshot.NewVersion != null:
                newSnapshot = snapshot with
                {
                    NewVersion = snapshot.NewVersion with { Data = e.Data.UseSameFields(Data()) }
                };
                break;

            case ContentUpdated e:
                newSnapshot = snapshot with
                {
                    CurrentVersion = snapshot.CurrentVersion with { Data = e.Data.UseSameFields(CurrentData()) }
                };
                break;

            case ContentSchedulingCancelled:
                newSnapshot = snapshot with { ScheduleJob = null };
                break;

            case ContentStatusScheduled e:
                newSnapshot = snapshot with { ScheduleJob = ScheduleJob.Build(e.Status, e.Actor, e.DueTime) };
                break;

            case ContentDeleted:
                newSnapshot = snapshot with { IsDeleted = true };
                break;
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
