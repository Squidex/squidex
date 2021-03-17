// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed partial class ContentDomainObject : DomainObject<ContentDomainObject.State>
    {
        private readonly ContentOperationContext context;

        public ContentDomainObject(IStore<DomainId> store, ISemanticLog log,
            ContentOperationContext context)
            : base(store, log)
        {
            Guard.NotNull(context, nameof(context));

            this.context = context;

            Capacity = int.MaxValue;
        }

        private Task LoadContext(ContentCommand command, bool optimize)
        {
            return context.LoadAsync(command.AppId, command.SchemaId, command, optimize);
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is CreateContent || command is UpsertContent;
        }

        protected override bool CanRecreate()
        {
            return true;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is ContentCommand contentCommand &&
                Equals(contentCommand.AppId, Snapshot.AppId) &&
                Equals(contentCommand.SchemaId, Snapshot.SchemaId) &&
                Equals(contentCommand.ContentId, Snapshot.Id);
        }

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case UpsertContent upsertContent:
                    return UpsertReturnAsync(upsertContent, async c =>
                    {
                        await LoadContext(c, c.OptimizeValidation);

                        if (Version > EtagVersion.Empty && !IsDeleted())
                        {
                            await UpdateCore(c.AsUpdate(), x => c.Data, false);
                        }
                        else
                        {
                            await CreateCore(c.AsCreate());
                        }

                        if (Is.OptionalChange(Snapshot.EditingStatus, c.Status))
                        {
                            await ChangeCore(c.AsChange(c.Status.Value));
                        }

                        return Snapshot;
                    });

                case CreateContent createContent:
                    return CreateReturnAsync(createContent, async c =>
                    {
                        await LoadContext(c, false);

                        await CreateCore(c);

                        // Skip validation for singleton contents because it is published from command middleware.
                        if (context.Schema.SchemaDef.IsSingleton)
                        {
                            ChangeStatus(c.AsChange(Status.Published));
                        }
                        else if (Is.OptionalChange(Snapshot.EditingStatus, c.Status))
                        {
                            await ChangeCore(c.AsChange(c.Status.Value));
                        }

                        return Snapshot;
                    });

                case ValidateContent validate:
                    return UpdateReturnAsync(validate, async c =>
                    {
                        await LoadContext(c, false);

                        GuardContent.CanValidate(c, Snapshot);

                        await context.ValidateContentAndInputAsync(Snapshot.Data);

                        return true;
                    });

                case CreateContentDraft createDraft:
                    return UpdateReturnAsync(createDraft, async c =>
                    {
                        await LoadContext(c, false);

                        GuardContent.CanCreateDraft(c, Snapshot);

                        var status = await context.GetInitialStatusAsync();

                        CreateDraft(c, status);

                        return Snapshot;
                    });

                case DeleteContentDraft deleteDraft:
                    return UpdateReturnAsync(deleteDraft, async c =>
                    {
                        await LoadContext(c, false);

                        GuardContent.CanDeleteDraft(c, Snapshot);

                        DeleteDraft(c);

                        return Snapshot;
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, async c =>
                    {
                        await LoadContext(c, c.OptimizeValidation);

                        await UpdateCore(c, c.Data.MergeInto, true);

                        return Snapshot;
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        await LoadContext(c, c.OptimizeValidation);

                        await UpdateCore(c, x => c.Data, false);

                        return Snapshot;
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, async c =>
                    {
                        try
                        {
                            await LoadContext(c, c.OptimizeValidation);

                            if (c.DueTime > SystemClock.Instance.GetCurrentInstant())
                            {
                                ChangeStatusScheduled(c, c.DueTime.Value);
                            }
                            else
                            {
                                await ChangeCore(c);
                            }
                        }
                        catch (Exception)
                        {
                            if (Snapshot.ScheduleJob != null && Snapshot.ScheduleJob.Id == c.StatusJobId)
                            {
                                CancelChangeStatus(c);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        return Snapshot;
                    });

                case DeleteContent deleteContent when (deleteContent.Permanent):
                    return DeletePermanentAsync(deleteContent, async c =>
                    {
                        await DeleteCore(c);
                    });

                case DeleteContent deleteContent:
                    return UpdateAsync(deleteContent, async c =>
                    {
                        await DeleteCore(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task CreateCore(CreateContent c)
        {
            var status = await context.GetInitialStatusAsync();

            GuardContent.CanCreate(c, context.Schema);

            var dataNew = c.Data;

            if (!c.DoNotValidate)
            {
                await context.ValidateInputAsync(dataNew);
            }

            if (!c.DoNotScript)
            {
                dataNew = await context.ExecuteScriptAndTransformAsync(s => s.Create,
                    new ScriptVars
                    {
                        Operation = "Create",
                        Data = dataNew,
                        Status = status,
                        StatusOld = default
                    });
            }

            await context.GenerateDefaultValuesAsync(dataNew);

            if (!c.DoNotValidate)
            {
                await context.ValidateContentAsync(dataNew);
            }

            Create(c, dataNew, status);
        }

        private async Task ChangeCore(ChangeContentStatus c)
        {
            await GuardContent.CanChangeStatus(c, Snapshot, context.Workflow, context.Repository, context.Schema);

            if (c.Status == Snapshot.EditingStatus)
            {
                return;
            }

            // Check for script to skip cloning if no script configured.
            if (!c.DoNotScript && context.HasScript(c => c.Change))
            {
                var change = GetChange(c.Status);

                // Clone the data, so that we do not change it in cases of errors.
                var data = Snapshot.Data.Clone();

                var newData = await context.ExecuteScriptAndTransformAsync(s => s.Change,
                    new ScriptVars
                    {
                        Operation = change.ToString(),
                        Data = data,
                        Status = c.Status,
                        StatusOld = Snapshot.EditingStatus
                    });

                // Just update the previous data event to improve performance and add less events.
                var previousEvent =
                    GetUncomittedEvents().Select(x => x.Payload)
                        .OfType<ContentDataCommand>().FirstOrDefault();

                if (previousEvent != null)
                {
                    previousEvent.Data = newData;
                }
                else if (!newData.Equals(Snapshot.Data))
                {
                    Update(c, newData);
                }
            }

            if (!c.DoNotValidate && c.Status == Status.Published)
            {
                await context.ValidateOnPublishAsync(Snapshot.Data);
            }

            ChangeStatus(c);
        }

        private async Task UpdateCore(UpdateContent c, Func<ContentData, ContentData> update, bool partial)
        {
            await GuardContent.CanUpdate(c, Snapshot, context.Workflow);

            var newData = update(Snapshot.Data);

            if (newData.Equals(Snapshot.Data))
            {
                return;
            }

            if (!c.DoNotValidate)
            {
                if (partial)
                {
                    await context.ValidateInputPartialAsync(c.Data);
                }
                else
                {
                    await context.ValidateInputAsync(c.Data);
                }
            }

            if (!c.DoNotScript)
            {
                newData = await context.ExecuteScriptAndTransformAsync(s => s.Update,
                    new ScriptVars
                    {
                        Operation = "Update",
                        Data = newData,
                        DataOld = Snapshot.Data,
                        Status = Snapshot.EditingStatus,
                        StatusOld = default
                    });
            }

            if (!c.DoNotValidate)
            {
                await context.ValidateContentAsync(newData);
            }

            Update(c, newData);
        }

        private async Task DeleteCore(DeleteContent c)
        {
            await LoadContext(c, false);

            await GuardContent.CanDelete(c, Snapshot, context.Repository, context.Schema);

            if (!c.DoNotScript)
            {
                await context.ExecuteScriptAsync(s => s.Delete,
                    new ScriptVars
                    {
                        Operation = "Delete",
                        Data = Snapshot.Data,
                        Status = Snapshot.EditingStatus,
                        StatusOld = default
                    });
            }

            Delete(c);
        }

        private void Create(CreateContent command, ContentData data, Status status)
        {
            Raise(command, new ContentCreated { Data = data, Status = status });
        }

        private void Update(ContentCommand command, ContentData data)
        {
            Raise(command, new ContentUpdated { Data = data });
        }

        private void ChangeStatus(ChangeContentStatus command)
        {
            Raise(command, new ContentStatusChanged { Change = GetChange(command.Status) });
        }

        private void ChangeStatusScheduled(ChangeContentStatus command, Instant dueTime)
        {
            Raise(command, new ContentStatusScheduled { DueTime = dueTime });
        }

        private void CancelChangeStatus(ChangeContentStatus command)
        {
            Raise(command, new ContentSchedulingCancelled());
        }

        private void CreateDraft(CreateContentDraft command, Status status)
        {
            Raise(command, new ContentDraftCreated { Status = status });
        }

        private void Delete(DeleteContent command)
        {
            Raise(command, new ContentDeleted());
        }

        private void DeleteDraft(DeleteContentDraft command)
        {
            Raise(command, new ContentDraftDeleted());
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
        {
            RaiseEvent(Envelope.Create(SimpleMapper.Map(command, @event)));
        }

        private StatusChange GetChange(Status status)
        {
            if (status == Status.Published)
            {
                return StatusChange.Published;
            }
            else if (Snapshot.EditingStatus == Status.Published)
            {
                return StatusChange.Unpublished;
            }
            else
            {
                return StatusChange.Change;
            }
        }
    }
}
