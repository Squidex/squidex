// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class TagByGroupNameProcessor : IOperationProcessor
    {
        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var groupName = context.ControllerType.GetCustomAttribute<ApiExplorerSettingsAttribute>()?.GroupName;

            if (!string.IsNullOrWhiteSpace(groupName))
            {
                context.OperationDescription.Operation.Tags = new List<string> { groupName };
            }

            return TaskHelper.True;
        }
    }
}
