// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using GraphQL;
using GraphQL.Execution;
using GraphQL.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class TestQuery
{
    private static readonly GraphQLSerializer Serializer = new GraphQLSerializer(TestUtils.DefaultOptions());

    required public string Query { get; set; }

    public object? Variables { get; set; }

    public object? Args { get; set; }

    public string[] Permissions { get; set; } = [];

    public string? OperationName { get; set; }

    public ExecutionOptions ToOptions(IServiceProvider services)
    {
        var query = Query;

        if (Args != null)
        {
            foreach (var property in Serialize(Args).EnumerateObject())
            {
                query = query.Replace($"{{{property.Name}}}", property.Value.ToString(), StringComparison.Ordinal);
            }
        }

        query = query.Replace('\'', '\"');

        var userPermissions = Permissions.Select(p => PermissionIds.ForApp(p, TestApp.Default.Name, TestSchemas.Default.Name).Id);
        var userPrincipal = Mocks.FrontendUser(null, userPermissions.ToArray());

        var context = new Context(userPrincipal, TestApp.Default);

        var options = new ExecutionOptions
        {
            Query = query,
            User = userPrincipal,
            UserContext = ActivatorUtilities.CreateInstance<GraphQLExecutionContext>(services, context),
            OperationName = OperationName,
        };

        if (Variables != null)
        {
            options.Variables = Serializer.ReadNode<Inputs>(Serialize(Variables))!;
        }

        foreach (var listener in services.GetRequiredService<IEnumerable<IDocumentExecutionListener>>())
        {
            options.Listeners.Add(listener);
        }

        return options;
    }

    private static JsonElement Serialize(object value)
    {
        return JsonSerializer.SerializeToElement(value, TestUtils.DefaultOptions());
    }
}
