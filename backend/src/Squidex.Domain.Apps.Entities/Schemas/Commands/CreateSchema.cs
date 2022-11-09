// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public sealed class CreateSchema : SchemaCommandBase, IUpsertCommand, IAggregateCommand
{
    public DomainId SchemaId { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public bool IsPublished { get; set; }

    public SchemaType Type { get; set; }

    public UpsertSchemaField[]? Fields { get; set; }

    public FieldNames? FieldsInReferences { get; set; }

    public FieldNames? FieldsInLists { get; set; }

    public FieldRuleCommand[]? FieldRules { get; set; }

    public SchemaScripts? Scripts { get; set; }

    public SchemaProperties Properties { get; set; }

    public ReadonlyDictionary<string, string>? PreviewUrls { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, SchemaId);
    }

    public CreateSchema()
    {
        SchemaId = DomainId.NewGuid();
    }

    public Schema BuildSchema()
    {
        return ((IUpsertCommand)this).ToSchema(Name, Type);
    }
}
