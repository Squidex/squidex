// ==========================================================================
//  HidePropertyFilter.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using PinkParrot.Infrastructure;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace PinkParrot.Pipeline.Swagger
{
    public class HidePropertyFilter : ISchemaFilter, IOperationFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            foreach (var property in context.JsonContract.UnderlyingType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<HideAttribute>();

                if (attribute != null)
                {
                    model.Properties.Remove(property.Name);
                }
            }
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context?.ApiDescription.ParameterDescriptions == null)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameterDescription in context.ApiDescription.ParameterDescriptions)
            {
                var metadata = parameterDescription.ModelMetadata as DefaultModelMetadata;

                var hasAttribute = metadata?.Attributes?.Attributes.OfType<HideAttribute>().Any();

                if (hasAttribute != true)
                {
                    continue;
                }

                var parameter = operation.Parameters.FirstOrDefault(p => p.Name == parameterDescription.Name);

                if (parameter != null)
                {
                    operation.Parameters.Remove(parameter);
                }
            }
        }
    }
}
