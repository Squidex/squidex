// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppMutationsGraphType : ObjectGraphType
    {
        public AppMutationsGraphType(IGraphModel model, IEnumerable<ISchemaEntity> schemas)
        {
            foreach (var schema in schemas)
            {
                var appId = schema.AppId;

                var schemaId = schema.NamedId();
                var schemaType = schema.TypeName();
                var schemaName = schema.DisplayName();

                var contentType = model.GetContentType(schema.Id);

                var inputType = new ContentDataInputGraphType(schema, schemaName, schemaType, model);

                AddField(new FieldType
                {
                    Name = $"create{schemaType}Content",
                    Arguments = ContentActions.Create.Arguments(inputType),
                    ResolvedType = contentType,
                    Resolver = ContentActions.Create.Resolver(appId, schemaId),
                    Description = $"Creates an {schemaName} content."
                });

                AddField(new FieldType
                {
                    Name = $"update{schemaType}Content",
                    Arguments = ContentActions.Update.Arguments(inputType),
                    ResolvedType = contentType,
                    Resolver = ContentActions.Update.Resolver(appId, schemaId),
                    Description = $"Update an {schemaName} content by id."
                });

                AddField(new FieldType
                {
                    Name = $"upsert{schemaType}Content",
                    Arguments = ContentActions.Upsert.Arguments(inputType),
                    ResolvedType = contentType,
                    Resolver = ContentActions.Upsert.Resolver(appId, schemaId),
                    Description = $"Upsert an {schemaName} content by id."
                });

                AddField(new FieldType
                {
                    Name = $"patch{schemaType}Content",
                    Arguments = ContentActions.Patch.Arguments(inputType),
                    ResolvedType = contentType,
                    Resolver = ContentActions.Patch.Resolver(appId, schemaId),
                    Description = $"Patch an {schemaName} content by id."
                });

                AddField(new FieldType
                {
                    Name = $"change{schemaType}Content",
                    Arguments = ContentActions.ChangeStatus.Arguments,
                    ResolvedType = contentType,
                    Resolver = ContentActions.ChangeStatus.Resolver(appId, schemaId),
                    Description = $"Change a {schemaName} content."
                });

                AddField(new FieldType
                {
                    Name = $"delete{schemaType}Content",
                    Arguments = ContentActions.Delete.Arguments,
                    ResolvedType = EntitySavedGraphType.NonNull,
                    Resolver = ContentActions.Delete.Resolver(appId, schemaId),
                    Description = $"Delete an {schemaName} content."
                });

                AddField(new FieldType
                {
                    Name = $"publish{schemaType}Content",
                    Arguments = ContentActions.ChangeStatus.Arguments,
                    ResolvedType = contentType,
                    Resolver = ContentActions.ChangeStatus.Resolver(appId, schemaId),
                    Description = $"Publish a {schemaName} content.",
                    DeprecationReason = $"Use 'change{schemaType}Content' instead"
                });
            }

            Description = "The app mutations.";
        }
    }
}
