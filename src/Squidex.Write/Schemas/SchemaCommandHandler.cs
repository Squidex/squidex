// ==========================================================================
//  SchemaCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Schemas.Commands;

namespace Squidex.Write.Schemas
{
    public class SchemaCommandHandler : CommandHandler<SchemaDomainObject>
    {
        private readonly ISchemaProvider schemaProvider;

        public SchemaCommandHandler(
            IDomainObjectFactory domainObjectFactory,
            IDomainObjectRepository domainObjectRepository,
            ISchemaProvider schemaProvider)
            : base(domainObjectFactory, domainObjectRepository)
        {
            this.schemaProvider = schemaProvider;
        }

        protected async Task On(CreateSchema command, CommandContext context)
        {
            if (await schemaProvider.FindSchemaIdByNameAsync(command.AppId, command.Name) != null)
            {
                var error = new ValidationError($"A schema with name '{command.Name}' already exists", "DisplayName");

                throw new ValidationException("Cannot create a new schema", error);
            }
            await CreateAsync(command, s =>
            {
                s.Create(command);

                context.Succeed(command.Name);
            });
        }

        protected Task On(AddField command, CommandContext context)
        {
            return UpdateAsync(command, s =>
            {
                s.AddField(command);

                context.Succeed(s.Schema.Fields.Values.First(x => x.Name == command.Name).Id);
            });
        }

        protected Task On(DeleteSchema command, CommandContext context)
        {
            return UpdateAsync(command, s => s.Delete(command));
        }

        protected Task On(DeleteField command, CommandContext context)
        {
            return UpdateAsync(command, s => s.DeleteField(command));
        }

        protected Task On(DisableField command, CommandContext context)
        {
            return UpdateAsync(command, s => s.DisableField(command));
        }

        protected Task On(EnableField command, CommandContext context)
        {
            return UpdateAsync(command, s => s.EnableField(command));
        }

        protected Task On(HideField command, CommandContext context)
        {
            return UpdateAsync(command, s => s.HideField(command));
        }

        protected Task On(ShowField command, CommandContext context)
        {
            return UpdateAsync(command, s => s.ShowField(command));
        }

        protected Task On(UpdateSchema command, CommandContext context)
        {
            return UpdateAsync(command, s => s.Update(command));
        }

        protected Task On(UpdateField command, CommandContext context)
        {
            return UpdateAsync(command, s => { s.UpdateField(command); });
        }

        public override Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command, context);
        }
    }
}
