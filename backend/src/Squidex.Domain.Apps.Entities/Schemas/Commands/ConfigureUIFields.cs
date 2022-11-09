// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public sealed class ConfigureUIFields : SchemaCommand
{
    public FieldNames? FieldsInLists { get; set; }

    public FieldNames? FieldsInReferences { get; set; }
}
