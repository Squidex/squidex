﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class AddFieldDto
    {
        /// <summary>
        /// The name of the field. Must be unique within the schema.
        /// </summary>
        [Required]
        [RegularExpression("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Determines the optional partitioning of the field.
        /// </summary>
        public string Partitioning { get; set; }

        /// <summary>
        /// The field properties.
        /// </summary>
        [Required]
        public FieldPropertiesDto Properties { get; set; }

        public AddField ToCommand(long? parentId = null)
        {
            return SimpleMapper.Map(this, new AddField { ParentFieldId = parentId, Properties = Properties.ToProperties() });
        }
    }
}