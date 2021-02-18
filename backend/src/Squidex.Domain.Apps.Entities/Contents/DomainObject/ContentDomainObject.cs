// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    public sealed partial class ContentDomainObject : LogSnapshotDomainObject<ContentDomainObject.State>
    {
        private readonly ContentOperationContext context;

        public ContentDomainObject(IStore<DomainId> store, ISemanticLog log,
            ContentOperationContext context)
            : base(store, log)
        {
            Guard.NotNull(context, nameof(context));

            this.context = context;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is CreateContent || command is UpsertContent;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is ContentCommand contentCommand &&
                Equals(contentCommand.AppId, Snapshot.AppId) &&
                Equals(contentCommand.SchemaId, Snapshot.SchemaId) &&
                Equals(contentCommand.ContentId, Snapshot.Id);
        }

        public override Task<object?> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case UpsertContent upsertContent:
                    return UpsertReturnAsync(upsertContent, async c =>
                    {
                        await LoadContext(c, c.OptimizeValidation);

                        if (Version > EtagVersion.Empty)
                        {
                            await UpdateAsync(c, x => c.Data, false);
                        }
                        else
                        {
                            var create = SimpleMapper.Map(c, new CreateContent());

                            await CreateAsync(create);
                        }

                        if (Is.OptionalChange(Snapshot.EditingStatus, c.Status))
                        {
                            var changeStatus = SimpleMapper.Map(c, new ChangeContentStatus
                            {
                                Status = c.Status.Value
                            });

                            await ChangeStatusAsync(changeStatus);
                        }

                        return Snapshot;
                    });

                case CreateContent createContent:
                    return CreateReturnAsync(createContent, async c =>
                    {
                        await LoadContext(c, false);

                        await CreateAsync(c);

                        if (c.Status != null && c.Status != Snapshot.Status)
                        {
                            var changeStatus = SimpleMapper.Map(c, new ChangeContentStatus
                            {
                                Status = c.Status.Value
                            });

                            await ChangeStatusAsync(changeStatus);
                        }

                        return Snapshot;
                    });

                case ValidateContent validateContent:
                    return UpdateReturnAsync(validateContent, async c =>
                    {
                        await LoadContext(c, false);

                        GuardContent.CanValidate(c, Snapshot);

                        await context.ValidateContentAndInputAsync(Snapshot.Data);

                        return true;
                    });

                case CreateContentDraft createContentDraft:
                    return UpdateReturnAsync(createContentDraft, async c =>
                    {
                        await LoadContext(c, false);

                        GuardContent.CanCreateDraft(c, Snapshot);

                        var status = await context.GetInitialStatusAsync();

                        CreateDraft(c, status);

                        return Snapshot;
                    });

                case DeleteContentDraft deleteContentDraft:
                    return UpdateReturnAsync(deleteContentDraft, async c =>
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

                        await UpdateAsync(c, c.Data.MergeInto, true);

                        return Snapshot;
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        await LoadContext(c, c.OptimizeValidation);

                        await UpdateAsync(c, x => c.Data, false);

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
                                ScheduleStatus(c, c.DueTime.Value);
                            }
                            else
                            {
                                await ChangeStatusAsync(c);
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

                case DeleteContent deleteContent:
                    return UpdateAsync(deleteContent, async c =>
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
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private async Task CreateAsync(CreateContent c)
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

        private async Task ChangeStatusAsync(ChangeContentStatus c)
        {
            await GuardContent.CanChangeStatus(c, Snapshot, context.Workflow, context.Repository, context.Schema);

            if (c.Status != Snapshot.Status)
            {
                if (!c.DoNotScript && context.HasScript(c => c.Change))
                {
                    var change = GetChange(c.Status);

                    var data = Snapshot.Data.Clone();

                    var newData = await context.ExecuteScriptAndTransformAsync(s => s.Change,
                        new ScriptVars
                        {
                            Operation = change.ToString(),
                            Data = data,
                            Status = c.Status,
                            StatusOld = Snapshot.EditingStatus
                        });

                    if (!newData.Equals(Snapshot.Data))
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
        }

        private async Task UpdateAsync(UpdateContent c, Func<ContentData, ContentData> newDataFunc, bool partial)
        {
            await GuardContent.CanUpdate(c, Snapshot, context.Workflow);

            var dataNew = newDataFunc(Snapshot.Data);

            if (!dataNew.Equals(Snapshot.Data))
            {
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
                    dataNew = await context.ExecuteScriptAndTransformAsync(s => s.Update,
                        new ScriptVars
                        {
                            Operation = "Update",
                            Data = dataNew,
                            DataOld = Snapshot.Data,
                            Status = Snapshot.EditingStatus,
                            StatusOld = default
                        });
                }

                if (!c.DoNotValidate)
                {
                    await context.ValidateContentAsync(dataNew);
                }

                Update(c, dataNew);
            }
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

        private void CancelChangeStatus(ChangeContentStatus command)
        {
            Raise(command, new ContentSchedulingCancelled());
        }

        private void ScheduleStatus(ChangeContentStatus command, Instant dueTime)
        {
            Raise(command, new ContentStatusScheduled { DueTime = dueTime });
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

        private Task LoadContext(ContentCommand command, bool optimize)
        {
            return context.LoadAsync(command.AppId, command.SchemaId, command, optimize);
        }
    }
}
