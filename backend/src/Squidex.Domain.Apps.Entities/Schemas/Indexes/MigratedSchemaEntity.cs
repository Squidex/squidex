// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes;

internal sealed class MigratedSchemaEntity : ISchemaEntity
{
    public NamedId<DomainId> AppId { get; set; }

    public bool IsDeleted { get; set; }

    public Schema SchemaDef { get; set; }

    public Instant Created { get; set; }

    public Instant LastModified { get; set; }

    public DomainId UniqueId { get; set; }

    public DomainId Id { get; set; }

    public RefToken CreatedBy { get; set; }

    public RefToken LastModifiedBy { get; set; }

    public long Version { get; set; }

    public static ISchemaEntity Migrate(ISchemaEntity schema)
    {
        var fieldsInLists = schema.SchemaDef.FieldsInLists;
        var fieldsInRefs = schema.SchemaDef.FieldsInReferences;

        var migratedFieldsInLists = fieldsInLists.Migrate();
        var migratedFieldsInRefs = fieldsInRefs.Migrate();

        var newSchemaDef = schema.SchemaDef;

        if (!ReferenceEquals(fieldsInLists, migratedFieldsInLists))
        {
            newSchemaDef = newSchemaDef.SetFieldsInLists(migratedFieldsInLists);
        }

        if (!ReferenceEquals(fieldsInRefs, migratedFieldsInRefs))
        {
            newSchemaDef = newSchemaDef.SetFieldsInReferences(migratedFieldsInRefs);
        }

        if (ReferenceEquals(newSchemaDef, schema.SchemaDef))
        {
            return schema;
        }

        var migrated = SimpleMapper.Map(schema, new MigratedSchemaEntity());

        migrated.SchemaDef = newSchemaDef;

        return migrated;
    }
}
