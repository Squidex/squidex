// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class ConfigureUIFieldsDto
{
    /// <summary>
    /// The name of fields that are used in content lists.
    /// </summary>
    public FieldNames? FieldsInLists { get; set; }

    /// <summary>
    /// The name of fields that are used in content references.
    /// </summary>
    public FieldNames? FieldsInReferences { get; set; }

    public ConfigureUIFields ToCommand()
    {
        return SimpleMapper.Map(this, new ConfigureUIFields());
    }
}
