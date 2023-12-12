// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class ApplicationMutations : ObjectGraphType
{
    public ApplicationMutations(Builder builder, IEnumerable<SchemaInfo> schemas)
    {
        foreach (var schemaInfo in schemas.Where(x => x.Fields.Count > 0))
        {
            var inputType = new DataInputGraphType(builder, schemaInfo);

            // We cannot check before if all fields can be resolved. This might not be the case, e.g. if a reference is empty.
            if (inputType.Fields.Count == 0)
            {
                continue;
            }

            var contentType = builder.GetContentType(schemaInfo);

            if (contentType == null)
            {
                continue;
            }

            var nonNullContentType = new NonNullGraphType(contentType);

            // Calculate the named if once to avoid allocations.
            var schemaid = schemaInfo.Schema.NamedId();

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"create{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Create.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Create.Resolver,
                Description = $"Creates an {schemaInfo.DisplayName} content.",
                SchemaId = schemaInfo.Schema.NamedId()
            });

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"update{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Update.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Update.Resolver,
                Description = $"Update an {schemaInfo.DisplayName} content by id.",
                SchemaId = schemaInfo.Schema.NamedId()
            });

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"upsert{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Upsert.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Upsert.Resolver,
                Description = $"Upsert an {schemaInfo.DisplayName} content by id.",
                SchemaId = schemaInfo.Schema.NamedId()
            });

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"patch{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Patch.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Patch.Resolver,
                Description = $"Patch an {schemaInfo.DisplayName} content by id.",
                SchemaId = schemaInfo.Schema.NamedId()
            });

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"change{schemaInfo.TypeName}Content",
                Arguments = ContentActions.ChangeStatus.Arguments,
                ResolvedType = contentType,
                Resolver = ContentActions.ChangeStatus.Resolver,
                Description = $"Change a {schemaInfo.DisplayName} content.",
                SchemaId = schemaInfo.Schema.NamedId()
            });

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"delete{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Delete.Arguments,
                ResolvedType = EntitySavedGraphType.NonNull,
                Resolver = ContentActions.Delete.Resolver,
                Description = $"Delete an {schemaInfo.DisplayName} content.",
                SchemaId = schemaInfo.Schema.NamedId()
            });

            AddField(new FieldTypeWithSchemaNamedId
            {
                Name = $"publish{schemaInfo.TypeName}Content",
                Arguments = ContentActions.ChangeStatus.Arguments,
                ResolvedType = contentType,
                Resolver = ContentActions.ChangeStatus.Resolver,
                Description = $"Publish a {schemaInfo.DisplayName} content.",
                DeprecationReason = $"Use 'change{schemaInfo.TypeName}Content' instead",
                SchemaId = schemaInfo.Schema.NamedId()
            });
        }

        Description = "The app mutations.";
    }
}
