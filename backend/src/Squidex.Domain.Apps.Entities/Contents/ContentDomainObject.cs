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
using Squidex.Domain.Apps.Entities.Contents.Guards;
using Squidex.Domain.Apps.Entities.Contents.Operations;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentDomainObject : LogSnapshotDomainObject<ContentState>
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
                case UpsertContent uspertContent:
                    {
                        if (Version > EtagVersion.Empty)
                        {
                            var updateContent = SimpleMapper.Map(uspertContent, new UpdateContent());

                            return ExecuteAsync(updateContent);
                        }
                        else
                        {
                            var createContent = SimpleMapper.Map(uspertContent, new CreateContent());

                            return ExecuteAsync(createContent);
                        }
                    }

                case CreateContent createContent:
                    return CreateReturnAsync(createContent, async c =>
                    {
                        await LoadContext(c.AppId, c.SchemaId, c, c.OptimizeValidation);

                        await GuardContent.CanCreate(context.Schema, context.Workflow, c);

                        var status = await context.GetInitialStatusAsync();

                        if (!c.DoNotValidate)
                        {
                            await context.ValidateInputAsync(c.Data, createContent.Publish);
                        }

                        if (!c.DoNotScript)
                        {
                            c.Data = await context.ExecuteScriptAndTransformAsync(s => s.Create,
                                new ScriptVars
                                {
                                    Operation = "Create",
                                    Data = c.Data,
                                    Status = status,
                                    StatusOld = default
                                });
                        }

                        await context.GenerateDefaultValuesAsync(c.Data);

                        if (!c.DoNotValidate)
                        {
                            await context.ValidateContentAsync(c.Data);
                        }

                        if (!c.DoNotScript && c.Publish)
                        {
                            c.Data = await context.ExecuteScriptAndTransformAsync(s => s.Change,
                                new ScriptVars
                                {
                                    Operation = "Published",
                                    Data = c.Data,
                                    Status = Status.Published,
                                    StatusOld = default
                                });
                        }

                        Create(c, status);

                        return Snapshot;
                    });

                case CreateContentDraft createContentDraft:
                    return UpdateReturnAsync(createContentDraft, async c =>
                    {
                        await LoadContext(Snapshot.AppId, Snapshot.SchemaId, c);

                        GuardContent.CanCreateDraft(c, Snapshot);

                        var status = await context.GetInitialStatusAsync();

                        CreateDraft(c, status);

                        return Snapshot;
                    });

                case DeleteContentDraft deleteContentDraft:
                    return UpdateReturnAsync(deleteContentDraft, async c =>
                    {
                        await LoadContext(Snapshot.AppId, Snapshot.SchemaId, c);

                        GuardContent.CanDeleteDraft(c, Snapshot);

                        DeleteDraft(c);

                        return Snapshot;
                    });

                case UpdateContent updateContent:
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        await GuardContent.CanUpdate(Snapshot, context.Workflow, c);

                        return await UpdateAsync(c, x => c.Data, false);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, async c =>
                    {
                        await GuardContent.CanPatch(Snapshot, context.Workflow, c);

                        return await UpdateAsync(c, c.Data.MergeInto, true);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, async c =>
                    {
                        try
                        {
                            await LoadContext(Snapshot.AppId, Snapshot.SchemaId, c);

                            await GuardContent.CanChangeStatus(context.Schema, Snapshot, context.Workflow, c);

                            if (c.DueTime.HasValue)
                            {
                                ScheduleStatus(c, c.DueTime.Value);
                            }
                            else
                            {
                                var change = GetChange(c);

                                if (!c.DoNotScript && context.HasScript(c => c.Change))
                                {
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
                                        var command = SimpleMapper.Map(c, new UpdateContent { Data = newData });

                                        Update(command, newData);
                                    }
                                }

                                if (!c.DoNotValidate && change == StatusChange.Published)
                                {
                                    await context.ValidateOnPublishAsync(Snapshot.Data);
                                }

                                ChangeStatus(c, change);
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
                        await LoadContext(Snapshot.AppId, Snapshot.SchemaId, c);

                        await GuardContent.CanDelete(context.Schema, Snapshot, context.Repository, c);

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

        private async Task<object> UpdateAsync(ContentUpdateCommand command, Func<NamedContentData, NamedContentData> newDataFunc, bool partial)
        {
            var currentData = Snapshot.Data;

            var newData = newDataFunc(currentData!);

            if (!currentData!.Equals(newData))
            {
                await LoadContext(Snapshot.AppId, Snapshot.SchemaId, command, command.OptimizeValidation);

                if (!command.DoNotValidate)
                {
                    if (partial)
                    {
                        await context.ValidateInputPartialAsync(command.Data);
                    }
                    else
                    {
                        await context.ValidateInputAsync(command.Data, false);
                    }
                }

                if (!command.DoNotScript)
                {
                    newData = await context.ExecuteScriptAndTransformAsync(s => s.Update,
                        new ScriptVars
                        {
                            Operation = "Create",
                            Data = newData,
                            DataOld = currentData,
                            Status = Snapshot.EditingStatus,
                            StatusOld = default
                        });
                }

                if (!command.DoNotValidate)
                {
                    await context.ValidateContentAsync(newData);
                }

                Update(command, newData);
            }

            return Snapshot;
        }

        public void Create(CreateContent command, Status status)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentCreated { Status = status }));

            if (command.Publish)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published }));
            }
        }

        public void CreateDraft(CreateContentDraft command, Status status)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDraftCreated { Status = status }));
        }

        public void Delete(DeleteContent command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));
        }

        public void DeleteDraft(DeleteContentDraft command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentDraftDeleted()));
        }

        public void Update(ContentCommand command, NamedContentData data)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentUpdated { Data = data }));
        }

        public void ChangeStatus(ChangeContentStatus command, StatusChange change)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusChanged { Change = change }));
        }

        public void CancelChangeStatus(ChangeContentStatus command)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentSchedulingCancelled()));
        }

        public void ScheduleStatus(ChangeContentStatus command, Instant dueTime)
        {
            RaiseEvent(SimpleMapper.Map(command, new ContentStatusScheduled { DueTime = dueTime }));
        }

        private void RaiseEvent(SchemaEvent @event)
        {
            @event.AppId ??= Snapshot.AppId;
            @event.SchemaId ??= Snapshot.SchemaId;

            RaiseEvent(Envelope.Create(@event));
        }

        private StatusChange GetChange(ChangeContentStatus command)
        {
            var change = StatusChange.Change;

            if (command.Status == Status.Published)
            {
                change = StatusChange.Published;
            }
            else if (Snapshot.EditingStatus == Status.Published)
            {
                change = StatusChange.Unpublished;
            }

            return change;
        }

        private Task LoadContext(NamedId<DomainId> appId, NamedId<DomainId> schemaId, ContentCommand command, bool optimized = false)
        {
            return context.LoadAsync(appId, schemaId, command, optimized);
        }
    }
}
