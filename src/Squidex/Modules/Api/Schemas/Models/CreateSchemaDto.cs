// ==========================================================================
//  CreateSchemaDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;

namespace Squidex.Modules.Api.Schemas.Models
{
    public class CreateSchemaDto
    {
        public string Name { get; set; }
        
        public FieldProperties Properties { get; set; }
    }
}
