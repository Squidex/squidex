// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class AddFieldDto
    {
        /// <summary>
        /// The name of the field. Must be unique within the schema.
        /// </summary>
        [LocalizedRequired]
        [LocalizedRegularExpression("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Determines the optional partitioning of the field.
        /// </summary>
        public string? Partitioning { get; set; }

        /// <summary>
        /// The field properties.
        /// </summary>
        [LocalizedRequired]
        public FieldPropertiesDto Properties { get; set; }

        public AddField ToCommand(long? parentId = null)
        {
            return SimpleMapper.Map(this, new AddField { ParentFieldId = parentId, Properties = Properties.ToProperties() });
        }
    }
}