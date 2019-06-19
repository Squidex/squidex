// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields
{
    public sealed class UIFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary>
        /// The editor that is used to manage this field.
        /// </summary>
        public UIFieldEditor Editor { get; set; }

        public override FieldProperties ToProperties()
        {
            return SimpleMapper.Map(this, new UIFieldProperties());
        }
    }
}
