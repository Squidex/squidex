// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.AI;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Schemas;

public sealed class SchemasChatTool : IChatToolProvider
{
    private readonly IAppProvider appProvider;
    private readonly IJsonSerializer serializer;
    private readonly IUrlGenerator urlGenerator;

    public SchemasChatTool(IAppProvider appProvider, IJsonSerializer serializer, IUrlGenerator urlGenerator)
    {
        this.appProvider = appProvider;
        this.serializer = serializer;
        this.urlGenerator = urlGenerator;
    }

    public async IAsyncEnumerable<IChatTool> GetToolsAsync(ChatContext chatContext,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (chatContext is not AppChatContext appContext)
        {
            yield break;
        }

        var context = appContext.BaseContext;

        await Task.Yield();

        if (context.Allows(PermissionIds.AppSchemasRead))
        {
            yield return new DelegateChatTool(
                new ToolSpec("schemas", "Schemas", "Provides the schemas for the Squidex App."),
                async (_, ct) =>
                {
                    var schemas = await appProvider.GetSchemasAsync(context.App.Id, ct);

                    var result = new
                    {
                        Schemas = schemas.Select(x =>
                            new
                            {
                                x.Name,
                                x.IsPublished,
                                x.Type,
                                FieldCount = x.Fields.Count,
                                Url = urlGenerator.SchemaUI(context.App.NamedId(), x.NamedId())
                            }),
                        Url = urlGenerator.SchemasUI(context.App.NamedId())
                    };

                    var json = serializer.Serialize(result, true);

                    return json;
                });
        }
    }
}
