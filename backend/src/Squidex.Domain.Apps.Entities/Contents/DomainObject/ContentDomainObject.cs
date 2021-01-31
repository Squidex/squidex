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
                        await LoadContext(c, c.OptimizeValidation);

                        await GuardContent.CanCreate(c, context.Workflow, context.Schema);

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
                    return UpdateReturnAsync(updateContent, async c =>
                    {
                        await GuardContent.CanUpdate(c, Snapshot, context.Workflow);

                        return await UpdateAsync(c, x => c.Data, false);
                    });

                case PatchContent patchContent:
                    return UpdateReturnAsync(patchContent, async c =>
                    {
                        await GuardContent.CanPatch(c, Snapshot, context.Workflow);

                        return await UpdateAsync(c, c.Data.MergeInto, true);
                    });

                case ChangeContentStatus changeContentStatus:
                    return UpdateReturnAsync(changeContentStatus, async c =>
                    {
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
                                var change = GetChange(c.Status);

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

        private async Task<object> UpdateAsync(ContentUpdateCommand command, Func<ContentData, ContentData> newDataFunc, bool partial)
        {
            var currentData = Snapshot.Data;

            var newData = newDataFunc(currentData!);

            if (!currentData!.Equals(newData))
            {
                await LoadContext(command, command.OptimizeValidation);

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
            Raise(command, new ContentCreated { Status = status });

            if (command.Publish)
            {
                var published = Status.Published;

                Raise(command, new ContentStatusChanged { Status = published, Change = GetChange(published) });
            }
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

        public void Update(ContentCommand command, ContentData data)
        {
            Raise(command, new ContentUpdated { Data = data });
        }

        public void ChangeStatus(ChangeContentStatus command, StatusChange change)
        {
            Raise(command, new ContentStatusChanged { Change = change });
        }

        public void CancelChangeStatus(ChangeContentStatus command)
        {
            Raise(command, new ContentSchedulingCancelled());
        }

        public void ScheduleStatus(ChangeContentStatus command, Instant dueTime)
        {
            Raise(command, new ContentStatusScheduled { DueTime = dueTime });
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where TEvent : SchemaEvent where T : class
        {
            SimpleMapper.Map(command, @event);

            @event.AppId ??= Snapshot.AppId;
            @event.SchemaId ??= Snapshot.SchemaId;

            RaiseEvent(Envelope.Create(@event));
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
            return context.LoadAsync(Snapshot.AppId, Snapshot.SchemaId, command, optimized);
        }

        private Task LoadContext(CreateContent command, bool optimized = false)
        {
            return context.LoadAsync(command.AppId, command.SchemaId, command, optimized);
        }
    }
}
