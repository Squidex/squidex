// ==========================================================================
//  ModelSchemaCommandHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Write.Schema
{
    public class ModelSchemaCommandHandler : ICommandHandler
    {
        private readonly ModelFieldRegistry registry;
        private readonly JsonSerializer serializer;

        public ModelSchemaCommandHandler(ModelFieldRegistry registry, JsonSerializer serializer)
        {
            this.registry = registry;
            this.serializer = serializer;
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command, context);
        }

        public Task On(CreateModelSchema command, CommandContext context)
        {
            var schema = context.Factory.CreateNew<ModelSchemaDomainObject>(command.AggregateId);

            schema.Create(command.TenantId, command.Name, command.Properties);

            return context.Repository.SaveAsync(schema, command.AggregateId);
        }

        public Task On(DeleteModelSchema command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.Delete());
        }

        public Task On(DeleteModelField command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.DeleteField(command.FieldId));
        }

        public Task On(DisableModelField command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.DisableField(command.FieldId));
        }

        public Task On(EnableModelField command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.EnableField(command.FieldId));
        }

        public Task On(HideModelField command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.HideField(command.FieldId));
        }

        public Task On(ShowModelField command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.ShowField(command.FieldId));
        }

        public Task On(UpdateModelSchema command, CommandContext context)
        {
            return UpdateAsync(command, context, s => s.Update(command.Properties));
        }

        public Task On(AddModelField command, CommandContext context)
        {
            var propertiesType = registry.FindByTypeName(command.Type).PropertiesType;
            var propertiesValue = (IModelFieldProperties)command.Properties.ToObject(propertiesType, serializer);

            return UpdateAsync(command, context, s => s.AddField(command.Name, propertiesValue));
        }

        public Task On(UpdateModelField command, CommandContext context)
        {
            return UpdateAsync(command, context, s =>
            {
                var field = s.Schema.Fields.GetOrDefault(command.FieldId);

                if (field == null)
                {
                    throw new DomainObjectNotFoundException(command.FieldId.ToString(), typeof(ModelField));
                }

                var propertiesType = registry.FindByPropertiesType(field.RawProperties.GetType()).PropertiesType;
                var propertiesValue = (IModelFieldProperties)command.Properties.ToObject(propertiesType, serializer);

                s.UpdateField(command.FieldId, propertiesValue);
            });
        }

        private static async Task UpdateAsync(IAggregateCommand command, CommandContext context, Action<ModelSchemaDomainObject> updater)
        {
            var schema = await context.Repository.GetByIdAsync<ModelSchemaDomainObject>(command.AggregateId);

            updater(schema);

            await context.Repository.SaveAsync(schema, Guid.NewGuid());
        }
    }
}
