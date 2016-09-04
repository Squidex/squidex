// ==========================================================================
//  ModelSchemaRM.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using PinkParrot.Infrastructure;

namespace PinkParrot.Read.Models
{
    public sealed class ModelSchemaRM
    {
        [Hide]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Guid SchemaId { get; set; }
    }
}
