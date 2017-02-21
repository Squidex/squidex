// ==========================================================================
//  SchemaDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace Squidex.Controllers.Api.Schemas.Models
{
    public class SchemaDto
    {
        /// <summary>
        /// The id of the schema.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The name of the schema. Unique within the app.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Optional label for the editor.
        /// </summary>
        [StringLength(100)]
        public string Label { get; set; }

        /// <summary>
        /// Indicates if the schema is published.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// The user that has created the schema.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the schema.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The date and time when the schema has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the schema has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }
    }
}
