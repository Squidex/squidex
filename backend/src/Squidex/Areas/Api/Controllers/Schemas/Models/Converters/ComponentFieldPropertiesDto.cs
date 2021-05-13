// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    public sealed class ComponentFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The id of the embedded schemas.
        /// </summary>
        public ImmutableList<DomainId>? SchemaIds { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new ComponentFieldProperties());

            return result;
        }
    }
}
