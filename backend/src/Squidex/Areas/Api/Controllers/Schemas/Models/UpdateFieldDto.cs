// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class UpdateFieldDto
{
    /// <summary>
    /// The field properties.
    /// </summary>
    [LocalizedRequired]
    public FieldPropertiesDto Properties { get; set; }

    public UpdateField ToCommand(long id, long? parentId = null)
    {
        return new UpdateField { ParentFieldId = parentId, FieldId = id, Properties = Properties?.ToProperties()! };
    }
}
