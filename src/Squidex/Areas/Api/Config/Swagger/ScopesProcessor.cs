// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Config;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Areas.Api.Config.Swagger
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
