// ==========================================================================
//  ModelSchemaRM.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using PinkParrot.Infrastructure;

namespace PinkParrot.Read.Models
{
    public sealed class ModelSchemaRM
    {
        [Hide]
        [BsonId]
        public string Id { get; set; }

        [Required]
        [BsonElement]
        public Guid SchemaId { get; set; }

        [Required]
        [BsonElement]
        public string Name { get; set; }

        [Required]
        [BsonElement]
        public DateTime Created { get; set; }

        [Required]
        [BsonElement]
        public DateTime Modified { get; set; }

        [BsonElement]
        public string Label { get; set; }

        [BsonElement]
        public string Hints { get; set; }
    }
}
