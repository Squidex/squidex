// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

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

            // We cannot check before if all fields can be resolved. This might not be the case, e.g. if a reference is empty.
            if (contentType == null || contentType.Fields.Count == 0)
            {
                continue;
            }

            var nonNullContentType = new NonNullGraphType(contentType);

            AddField(new FieldType
            {
                Name = $"create{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Create.Arguments(inputType),
                ResolvedType = nonNullContentType,
                Resolver = ContentActions.Create.Resolver,
                Description = $"Creates an {schemaInfo.DisplayName} content."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"update{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Update.Arguments(inputType),
                ResolvedType = nonNullContentType,
                Resolver = ContentActions.Update.Resolver,
                Description = $"Update an {schemaInfo.DisplayName} content by id."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"upsert{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Upsert.Arguments(inputType),
                ResolvedType = nonNullContentType,
                Resolver = ContentActions.Upsert.Resolver,
                Description = $"Upsert an {schemaInfo.DisplayName} content by id."
            }).WithSchemaNamedId(schemaInfo);

            AddField(new FieldType
            {
                Name = $"patch{schemaInfo.TypeName}Content",
                Arguments = ContentActions.Patch.Arguments(inputType),
                ResolvedType = nonNullContentType,
                Resolver = ContentActions.Patch.Resolver,
                Description = $"Patch an {schemaInfo.DisplayName} content by id."
            }).WithSchemaNamedId(schemaInfo);
        }

        Description = "The app mutations.";
    }
}
