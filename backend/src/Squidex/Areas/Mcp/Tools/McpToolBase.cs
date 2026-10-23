// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using ModelContextProtocol;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

namespace Squidex.Areas.Mcp.Tools;

public abstract class McpToolBase(IContextProvider contextProvider, IJsonSerializer serializer)
{
    protected async Task<string> ExecuteAsync(Func<Context, Task<object?>> action)
    {
        var context = contextProvider.Context;

        // Should never happen, because the middleware resolves the app before.
        if (context.App == null)
        {
            throw new McpException("App not found.");
        }

        try
        {
            var result = await action(context);

            return serializer.Serialize(result);
        }
        catch (DomainException ex)
        {
            // Domain exceptions are also exposed by the REST API, so the message is safe to share.
            throw new McpException(ex.Message, ex);
        }
    }

    protected static void EnsurePermission(Context context, string permissionId)
    {
        if (!context.UserPermissions.Allows(permissionId, context.App.Name))
        {
            throw new McpException("You do not have the permission to perform this operation.");
        }
    }

    protected static DomainId ParseId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new McpException("The ID must not be empty.");
        }

        return DomainId.Create(id);
    }
}
