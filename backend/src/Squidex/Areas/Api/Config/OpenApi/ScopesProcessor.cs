// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class ScopesProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>();

        var permissionAttribute = context.MethodInfo.GetCustomAttribute<ApiPermissionAttribute>();

        if (permissionAttribute != null)
        {
            context.OperationDescription.Operation.Security.Add(new OpenApiSecurityRequirement
            {
                [Constants.SecurityDefinition] = permissionAttribute.PermissionIds
            });
        }
        else
        {
            var authorizeAttributes =
                context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(true).Union(
                context.MethodInfo.DeclaringType!.GetCustomAttributes<AuthorizeAttribute>(true))
                    .ToArray();

            if (authorizeAttributes.Any())
            {
                var scopes = authorizeAttributes.Where(a => a.Roles != null).SelectMany(a => a.Roles!.Split(',')).Distinct().ToList();

                context.OperationDescription.Operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [Constants.SecurityDefinition] = scopes
                });
            }
        }

        return true;
    }
}
