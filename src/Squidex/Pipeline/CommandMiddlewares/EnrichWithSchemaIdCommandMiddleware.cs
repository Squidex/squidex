// ==========================================================================
//  EnrichWithSchemaIdCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write;
using Squidex.Domain.Apps.Write.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public sealed class EnrichWithSchemaIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IAppProvider appProvider;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaIdCommandMiddleware(IAppProvider appProvider, IActionContextAccessor actionContextAccessor)
        {
            this.appProvider = appProvider;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is SchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemaName = routeValues["name"].ToString();

                    ISchemaEntity schema;

                    if (Guid.TryParse(schemaName, out var id))
                    {
                        schema = await appProvider.GetSchemaAsync(schemaCommand.AppId.Name, id);
                    }
                    else
                    {
                        schema = await appProvider.GetSchemaAsync(schemaCommand.AppId.Name, schemaName);
                    }

                    if (schema == null)
                    {
                        throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                    }

                    schemaCommand.SchemaId = new NamedId<Guid>(schema.Id, schema.Name);
                }
            }

            await next();
        }
    }
}
