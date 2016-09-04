// ==========================================================================
//  RemoveReadonlyFilter.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace PinkParrot.Pipeline.Swagger
{
    public class RemoveReadonlyFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            Apply(model);
        }

        private static void Apply(Schema model)
        {
            model.ReadOnly = null;

            if (model.Properties == null)
            {
                return;
            }

            foreach (var property in model.Properties)
            {
                Apply(property.Value);
            }
        }
    }
}
