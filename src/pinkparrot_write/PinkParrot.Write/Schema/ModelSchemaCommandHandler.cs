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
using Newtonsoft.Json.Linq;
using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Write.Schema
{
    public class ModelSchemaCommandHandler : CommandHandler<ModelSchemaDomainObject>
    {
        private readonly ModelFieldRegistry registry;
        private readonly JsonSerializer serializer;

        public ModelSchemaCommandHandler(
            ModelFieldRegistry registry,
            IDomainObjectFactory domainObjectFactory,
            IDomainObjectRepository domainObjectRepository,
            JsonSerializer serializer)
            : base(domainObjectFactory, domainObjectRepository)
        {
            this.registry = registry;

            this.serializer = serializer;
        }

        public override Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command);
        }

        public Task On(CreateModelSchema command)
        {
            return CreateAsync(command, s => s.Create(command.TenantId, command.Name, command.Properties));
        }

        public Task On(DeleteModelSchema command)
        {
            return UpdateAsync(command, s => s.Delete());
        }

        public Task On(DeleteModelField command)
        {
            return UpdateAsync(command, s => s.DeleteField(command.FieldId));
        }

        public Task On(DisableModelField command)
        {
            return UpdateAsync(command, s => s.DisableField(command.FieldId));
        }

        public Task On(EnableModelField command)
        {
            return UpdateAsync(command, s => s.EnableField(command.FieldId));
        }

        public Task On(HideModelField command)
        {
            return UpdateAsync(command, s => s.HideField(command.FieldId));
        }

        public Task On(ShowModelField command)
        {
            return UpdateAsync(command, s => s.ShowField(command.FieldId));
        }

        public Task On(UpdateModelSchema command)
        {
            return UpdateAsync(command, s => s.Update(command.Properties));
        }

        public Task On(AddModelField command)
        {
            var propertiesType = registry.FindByTypeName(command.Type).PropertiesType;
            var propertiesValue = CreateProperties(command.Properties, propertiesType);

            return UpdateAsync(command, s => s.AddField(command.Name, propertiesValue));
        }

        public Task On(UpdateModelField command)
        {
            return UpdateAsync(command, s =>
            {
                var field = s.Schema.Fields.GetOrDefault(command.FieldId);

                if (field == null)
                {
                    throw new DomainObjectNotFoundException(command.FieldId.ToString(), typeof(ModelField));
                }

                var propertiesType = registry.FindByPropertiesType(field.RawProperties.GetType()).PropertiesType;
                var propertiesValue = CreateProperties(command.Properties, propertiesType);

                s.UpdateField(command.FieldId, propertiesValue);
            });
        }

        private IModelFieldProperties CreateProperties(JToken token, Type type)
        {
            return (IModelFieldProperties)token.ToObject(type, serializer);
        }
    }
}
