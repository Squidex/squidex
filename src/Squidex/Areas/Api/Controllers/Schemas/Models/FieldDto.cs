// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class FieldDto
    {
        /// <summary>
        /// The id of the field.
        /// </summary>
        public long FieldId { get; set; }

        /// <summary>
        /// The name of the field. Must be unique within the schema.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Defines if the field is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Defines if the field is locked.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Defines if the field is disabled.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Defines the partitioning of the field.
        /// </summary>
        [Required]
        public string Partitioning { get; set; }

        /// <summary>
        /// The field properties.
        /// </summary>
        [Required]
        public FieldPropertiesDto Properties { get; set; }
    }
}
