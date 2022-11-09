// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class ApplicationMutations : ObjectGraphType
{
    public ApplicationMutations(Builder builder, IEnumerable<SchemaInfo> schemas)
    {
        foreach (var schemaInfo in schemas.Where(x => x.Fields.Count > 0))
        {
            var contentType = new NonNullGraphType(builder.GetContentType(schemaInfo));

            var inputType = new DataInputGraphType(builder, schemaInfo);

            AddField(new FieldType
            {
                Name = $"create{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Create.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Create.Resolver,
                Description = $"Creates an {schemaInfo.DisplayName} content."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"update{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Update.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Update.Resolver,
                Description = $"Update an {schemaInfo.DisplayName} content by id."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"upsert{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Upsert.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Upsert.Resolver,
                Description = $"Upsert an {schemaInfo.DisplayName} content by id."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"patch{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Patch.Arguments(inputType),
                ResolvedType = contentType,
                Resolver = ContentActions.Patch.Resolver,
                Description = $"Patch an {schemaInfo.DisplayName} content by id."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"change{schemaInfo.TypeName}Content",
                Arguments = ContentActions.ChangeStatus.Arguments,
                ResolvedType = contentType,
                Resolver = ContentActions.ChangeStatus.Resolver,
                Description = $"Change a {schemaInfo.DisplayName} content."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"delete{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Delete.Arguments,
                ResolvedType = EntitySavedGraphType.NonNull,
                Resolver = ContentActions.Delete.Resolver,
                Description = $"Delete an {schemaInfo.DisplayName} content."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"publish{schemaInfo.TypeName}Content",
                Arguments = ContentActions.ChangeStatus.Arguments,
                ResolvedType = contentType,
                Resolver = ContentActions.ChangeStatus.Resolver,
                Description = $"Publish a {schemaInfo.DisplayName} content.",
                DeprecationReason = $"Use 'change{schemaInfo.TypeName}Content' instead"
            }).WithSchemaNamedId(schemaInfo);
        }

        Description = "The app mutations.";
    }
}
