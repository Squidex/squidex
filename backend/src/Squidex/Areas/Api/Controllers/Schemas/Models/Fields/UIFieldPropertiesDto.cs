// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

[OpenApiRequest]
public sealed class UIFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The editor that is used to manage this field.
    /// </summary>
    public UIFieldEditor Editor { get; set; }

    public static UIFieldPropertiesDto FromDomain(UIFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new UIFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new UIFieldProperties());
    }
}
