// ==========================================================================
//  SchemaDetailsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Controllers.Api.Schemas.Models
{
    public sealed class SchemaDetailsDto
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
        /// Indicates if the schema is published.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// The script that is executed for each query when querying contents.
        /// </summary>
        public string ScriptQuery { get; set; }

        /// <summary>
        /// The script that is executed when creating a content.
        /// </summary>
        public string ScriptCreate { get; set; }

        /// <summary>
        /// The script that is executed when updating a content.
        /// </summary>
        public string ScriptUpdate { get; set; }

        /// <summary>
        /// The script that is executed when deleting a content.
        /// </summary>
        public string ScriptDelete { get; set; }

        /// <summary>
        /// The script that is executed when changing a content status.
        /// </summary>
        public string ScriptChange { get; set; }

        /// <summary>
        /// The list of fields.
        /// </summary>
        [Required]
        public List<FieldDto> Fields { get; set; }

        /// <summary>
        /// The schema properties.
        /// </summary>
        [Required]
        public SchemaPropertiesDto Properties { get; set; }

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

        /// <summary>
        /// The version of the schema.
        /// </summary>
        public int Version { get; set; }
    }
}
