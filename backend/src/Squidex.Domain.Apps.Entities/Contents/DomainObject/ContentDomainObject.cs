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
            return command is CreateContent;
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
                    return UpsertReturnAsync(upsertContent, c =>
                    {
                        if (Version > EtagVersion.Empty)
                        {
                            return UpdateAsync(c, x => c.Data, false);
                        }
                        else
                        {
                            return CreateAsync(c);
                        }
                    });

                case CreateContent createContent:
                    return CreateReturnAsync(createContent, c =>
                    {
                        return CreateAsync(c);
                    });

                case ValidateContent validateContent:
                    return UpdateReturnAsync(validateContent, async c =>
                    {
                        await LoadContext(c);

                        GuardContent.CanValidate(c, Snapshot);

                        await context.ValidateContentAndInputAsync(Snapshot.Data);

                        return true;
                    });

                case CreateContentDraft createContentDraft:
                    return UpdateReturnAsync(createContentDraft, async c =>
                    {
                        await LoadContext(c);

                        GuardContent.CanCreateDraft(c, Snapshot);

                        var status = await context.GetInitialStatusAsync();

                        CreateDraft(c, status);

                        return Snapshot;
                    });

                case DeleteContentDraft deleteContentDraft:
                    return UpdateReturnAsync(deleteContentDraft, async c =>
                    {
                        await LoadContext(c);

                        GuardContent.CanDeleteDraft(c, Snapshot);

                        DeleteDraft(c);

                        return Snapshot;
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, c =>
                    {
                        return UpdateAsync(c, x => c.Data, false);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, c =>
                    {
                        return UpdateAsync(c, c.Data.MergeInto, true);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, async c =>
                    {
                        if (c.Status == Snapshot.Status)
                        {
                            return Snapshot;
                        }

                        try
                        {
                            await LoadContext(c);

                            await GuardContent.CanChangeStatus(c, Snapshot, context.Workflow, context.Repository, context.Schema);

                            if (c.DueTime.HasValue)
                            {
                                ScheduleStatus(c, c.DueTime.Value);
                            }
                            else
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

                                ChangeStatus(c, c.Status);
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
                        await LoadContext(c);

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

        private async Task<object?> CreateAsync(ContentDataCommand c)
        {
            await LoadContext(c, c.OptimizeValidation);

            var statusInitial = await context.GetInitialStatusAsync();
            var statusFinal = c.Status ?? statusInitial;

            await GuardContent.CanCreate(c, statusInitial, context.Workflow, context.Schema);

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
                        Status = statusInitial,
                        StatusOld = default
                    });
            }

            await context.GenerateDefaultValuesAsync(dataNew);

            if (!c.DoNotValidate)
            {
                await context.ValidateContentAsync(dataNew);
            }

            if (c.Status != null && statusFinal != statusInitial)
            {
                if (!c.DoNotScript)
                {
                    var change = GetChange(statusFinal);

                    dataNew = await context.ExecuteScriptAndTransformAsync(s => s.Change,
                        new ScriptVars
                        {
                            Operation = change.ToString(),
                            Data = dataNew,
                            Status = statusFinal,
                            StatusOld = default
                        });
                }

                if (!c.DoNotValidate && statusFinal == Status.Published)
                {
                    await context.ValidateOnPublishAsync(Snapshot.Data);
                }
            }

            Create(c, dataNew, statusInitial);

            if (statusFinal != statusInitial)
            {
                ChangeStatus(c, statusFinal);
            }

            return Snapshot;
        }

        private async Task<object?> UpdateAsync(ContentDataCommand c, Func<ContentData, ContentData> newDataFunc, bool partial)
        {
            await LoadContext(c, c.OptimizeValidation);

            var dataOld = Snapshot.Data;
            var dataNew = newDataFunc(dataOld!);

            var changeStatus = c.Status != null && c.Status != Snapshot.Status;
            var changeData = !dataOld.Equals(dataNew);

            if (changeData || changeStatus)
            {
                await GuardContent.CanUpdate(c, Snapshot, context.Workflow, context.Repository, context.Schema);

                if (changeData)
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
                                DataOld = dataOld,
                                Status = Snapshot.EditingStatus,
                                StatusOld = default
                            });
                    }

                    if (!c.DoNotValidate)
                    {
                        await context.ValidateContentAsync(dataNew);
                    }
                }

                if (changeStatus)
                {
                    if (!c.DoNotScript && changeStatus)
                    {
                        var change = GetChange(c.Status!.Value);

                        dataNew = await context.ExecuteScriptAndTransformAsync(s => s.Change,
                            new ScriptVars
                            {
                                Operation = change.ToString(),
                                Data = dataNew,
                                Status = c.Status.Value,
                                StatusOld = default
                            });
                    }

                    if (!c.DoNotValidate && c.Status == Status.Published)
                    {
                        await context.ValidateOnPublishAsync(dataNew);
                    }
                }

                if (!dataOld.Equals(dataNew))
                {
                    Update(c, dataNew);
                }

                if (changeStatus)
                {
                    ChangeStatus(c, c.Status!.Value);
                }
            }

            return Snapshot;
        }

        public void Create(ContentCommand command, ContentData data, Status status)
        {
            Raise(command, new ContentCreated { Data = data, Status = status });
        }

        public void Update(ContentCommand command, ContentData data)
        {
            Raise(command, new ContentUpdated { Data = data });
        }

        public void ChangeStatus(ContentCommand command, Status status)
        {
            Raise(command, new ContentStatusChanged { Status = status, Change = GetChange(status) });
        }

        public void CreateDraft(CreateContentDraft command, Status status)
        {
            Raise(command, new ContentDraftCreated { Status = status });
        }

        public void Delete(DeleteContent command)
        {
            Raise(command, new ContentDeleted());
        }

        public void DeleteDraft(DeleteContentDraft command)
        {
            Raise(command, new ContentDraftDeleted());
        }

        public void CancelChangeStatus(ChangeContentStatus command)
        {
            Raise(command, new ContentSchedulingCancelled());
        }

        public void ScheduleStatus(ChangeContentStatus command, Instant dueTime)
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

        private Task LoadContext(ContentCommand command, bool optimized = false)
        {
            return context.LoadAsync(command.AppId, command.SchemaId, command, optimized);
        }
    }
}
