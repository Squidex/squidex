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

    public string? Permission { get; set; }

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

        var options = new ExecutionOptions
        {
            Query = query.Replace('\'', '\"')
        };

        if (OperationName != null)
        {
            options.OperationName = OperationName;
        }

        if (Variables != null)
        {
            options.Variables = Serializer.ReadNode<Inputs>(Serialize(Variables))!;
        }

        foreach (var listener in services.GetRequiredService<IEnumerable<IDocumentExecutionListener>>())
        {
            options.Listeners.Add(listener);
        }

        options.UserContext = ActivatorUtilities.CreateInstance<GraphQLExecutionContext>(services, BuildContext(Permission));

        return options;

        static Context BuildContext(string? permissionId)
        {
            if (permissionId == null)
            {
                return new Context(Mocks.FrontendUser(), TestApp.Default);
            }

            var permission = PermissionIds.ForApp(permissionId, TestApp.Default.Name, TestSchemas.Default.Name).Id;

            return new Context(Mocks.FrontendUser(permission: permission), TestApp.Default);
        }

        static JsonElement Serialize(object value)
        {
            return JsonSerializer.SerializeToElement(value, TestUtils.DefaultOptions());
        }
    }
}
