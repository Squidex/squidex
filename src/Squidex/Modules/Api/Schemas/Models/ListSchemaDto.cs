// ==========================================================================
//  ListSchemaDto.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Modules.Api.Schemas.Models
{
    public class ListSchemaDto
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime LastModified { get; set; }
    }
}
