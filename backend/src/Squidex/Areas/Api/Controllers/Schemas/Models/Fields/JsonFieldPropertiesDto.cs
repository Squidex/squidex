// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Fields;

public sealed class JsonFieldPropertiesDto : FieldPropertiesDto
{
    /// <summary>
    /// The GraphQL schema.
    /// </summary>
    public string? GraphQLSchema { get; set; }

    public override FieldProperties ToProperties()
    {
        var result = SimpleMapper.Map(this, new JsonFieldProperties());

        return result;
    }
}
