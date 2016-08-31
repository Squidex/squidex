// ==========================================================================
//  ModelSchemaCommandHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Write.Schema
{
    public class ModelSchemaCommandHandler : ICommandHandler
    {
        public Task<bool> HandleAsync(CommandContext context)
        {
            return this.DispatchActionAsync(context.Command, context);
        }

        protected Task On(AddModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.AddField(command));
        }

        protected Task On(DeleteModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.DeleteField(command));
        }

        protected Task On(DeleteModelSchema command, CommandContext context)
        {
            return Update(command, context, schema => schema.Delete(command));
        }

        protected Task On(DisableModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.DisableField(command));
        }

        protected Task On(EnableModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.EnableField(command));
        }

        protected Task On(HideModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.HideField(command));
        }

        protected Task On(ShowModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.ShowField(command));
        }

        protected Task On(UpdateModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.UpdateField(command));
        }

        protected Task On(UpdateModelSchema command, CommandContext context)
        {
            return Update(command, context, schema => schema.Update(command));
        }

        protected Task On(CreateModelSchema command, CommandContext context)
        {
            var schema = context.Factory.CreateNew<ModelSchemaDomainObject>(command.AggregateId);

            schema.Create(command);

            return context.Repository.SaveAsync(schema, Guid.NewGuid());
        }

        private async Task Update(AggregateCommand command, CommandContext context, Action<ModelSchemaDomainObject> updater)
        {
            var schema = await context.Repository.GetByIdAsync<ModelSchemaDomainObject>(command.AggregateId);

            updater(schema);

            await context.Repository.SaveAsync(schema, Guid.NewGuid());
        }
    }
}
