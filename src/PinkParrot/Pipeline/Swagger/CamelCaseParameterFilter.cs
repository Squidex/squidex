// ==========================================================================
//  CamelCaseParameterFilter.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace PinkParrot.Pipeline.Swagger
{
    public sealed class CamelCaseParameterFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                parameter.Name = char.ToLowerInvariant(parameter.Name[0]) + parameter.Name.Substring(1);
            }
        }
    }
}
