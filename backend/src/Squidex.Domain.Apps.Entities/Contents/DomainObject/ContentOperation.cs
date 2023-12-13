// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentOperation : OperationContextBase<ContentCommand, WriteContent>
{
    public Schema Schema { get; init; }

    public ResolvedComponents Components { get; init; }

    public ContentOperation(IServiceProvider serviceProvider, Func<WriteContent> snapshot)
        : base(serviceProvider, snapshot)
    {
    }

    public static async Task<ContentOperation> CreateAsync(IServiceProvider services, ContentCommand command, Func<WriteContent> snapshot)
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
