// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class CreateSchemaDto : UpsertSchemaDto
{
    /// <summary>
    /// The name of the schema.
    /// </summary>
    [LocalizedRequired]
    [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
    public string Name { get; set; }

    /// <summary>
    /// The type of the schema.
    /// </summary>
    public SchemaType Type { get; set; }

    /// <summary>
    /// Set to true to allow a single content item only.
    /// </summary>
    [Obsolete("Use 'type' field now.")]
    public bool IsSingleton
    {
        get => Type == SchemaType.Singleton;
        set
        {
            if (value)
            {
                Type = SchemaType.Singleton;
            }
        }
    }

    public CreateSchema ToCommand()
    {
        return ToCommand(this, new CreateSchema());
    }
}
