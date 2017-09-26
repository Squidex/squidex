// ==========================================================================
//  ScopesProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Config.Swagger
{
    public class ScopesProcessor : IOperationProcessor
    {
        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            if (context.OperationDescription.Operation.Security == null)
            {
                context.OperationDescription.Operation.Security = new List<SwaggerSecurityRequirement>();
            }

            var authorizeAttributes =
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Union(
                context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true).OfType<AuthorizeAttribute>()).ToArray();

            if (authorizeAttributes.Any())
            {
                var scopes = authorizeAttributes.Where(a => a.Roles != null).SelectMany(a => a.Roles.Split(',')).Distinct().ToList();

                context.OperationDescription.Operation.Security.Add(new SwaggerSecurityRequirement
                {
                    { Constants.SecurityDefinition, scopes }
                });
            }

            return TaskHelper.True;
        }
    }
}
