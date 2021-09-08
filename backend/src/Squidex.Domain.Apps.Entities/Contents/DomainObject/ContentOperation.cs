// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class ContentOperation : OperationContextBase<ContentCommand, IContentEntity>
    {
        public ISchemaEntity Schema { get; init; }

        public ResolvedComponents Components { get; init; }

        public Schema SchemaDef
        {
            get => Schema.SchemaDef;
        }

        public ContentOperation(IServiceProvider serviceProvider, Func<IContentEntity> snapshot)
            : base(serviceProvider, snapshot)
        {
        }

        public static async Task<ContentOperation> CreateAsync(IServiceProvider services, ContentCommand command, Func<IContentEntity> snapshot)
        {
            var appProvider = services.GetRequiredService<IAppProvider>();

            var (app, schema) = await appProvider.GetAppWithSchemaAsync(command.AppId.Id, command.SchemaId.Id);

            if (app == null)
            {
                throw new DomainObjectNotFoundException(command.AppId.Id.ToString());
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(command.SchemaId.Id.ToString());
            }

            var components = await appProvider.GetComponentsAsync(schema);

            return new ContentOperation(services, snapshot)
            {
                App = app,
                Command = command,
                CommandId = command.ContentId,
                Components = components,
                Schema = schema
            };
        }
    }
}
