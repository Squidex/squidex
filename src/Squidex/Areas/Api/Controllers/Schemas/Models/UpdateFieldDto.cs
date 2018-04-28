// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class UpdateFieldDto
    {
        /// <summary>
        /// The field properties.
        /// </summary>
        [Required]
        public FieldPropertiesDto Properties { get; set; }

        public UpdateField ToCommand(long id)
        {
            return new UpdateField { FieldId = id, Properties = Properties?.ToProperties() };
        }
    }
}
