// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class FieldTypeWithSchemaId : FieldType
{
    required public DomainId SchemaId { get; set; }
}

internal sealed class FieldTypeWithSchemaNamedId : FieldType
{
    required public NamedId<DomainId> SchemaId { get; set; }
}
