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
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command, context);
        }

        public Task On(AddModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.AddField(command));
        }

        public Task On(DeleteModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.DeleteField(command));
        }

        public Task On(DeleteModelSchema command, CommandContext context)
        {
            return Update(command, context, schema => schema.Delete(command));
        }

        public Task On(DisableModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.DisableField(command));
        }

        public Task On(EnableModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.EnableField(command));
        }

        public Task On(HideModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.HideField(command));
        }

        public Task On(ShowModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.ShowField(command));
        }

        public Task On(UpdateModelField command, CommandContext context)
        {
            return Update(command, context, schema => schema.UpdateField(command));
        }

        public Task On(UpdateModelSchema command, CommandContext context)
        {
            return Update(command, context, schema => schema.Update(command));
        }

        public Task On(CreateModelSchema command, CommandContext context)
        {
            var schema = context.Factory.CreateNew<ModelSchemaDomainObject>(command.AggregateId);

            schema.Create(command);

            return context.Repository.SaveAsync(schema, command.AggregateId);
        }

        private static async Task Update(IAggregateCommand command, CommandContext context, Action<ModelSchemaDomainObject> updater)
        {
            var schema = await context.Repository.GetByIdAsync<ModelSchemaDomainObject>(command.AggregateId);

            updater(schema);

            await context.Repository.SaveAsync(schema, Guid.NewGuid());
        }
    }
}
