// ==========================================================================
//  ModelSchemaRM.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Read.Models
{
    public sealed class ModelSchemaRM
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Guid SchemaId { get; set; }
    }
}
