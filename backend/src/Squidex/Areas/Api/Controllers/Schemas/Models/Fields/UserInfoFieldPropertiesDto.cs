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
public sealed class UserInfoFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The role to create a default value.
    /// </summary>
    public string? DefaultRole { get; init; }

    public static UserInfoFieldPropertiesDto FromDomain(UserInfoFieldProperties fieldProperties)
    {
        return SimpleMapper.Map(fieldProperties, new UserInfoFieldPropertiesDto());
    }

    public override FieldProperties ToProperties()
    {
        return SimpleMapper.Map(this, new UserInfoFieldProperties());
    }
}
