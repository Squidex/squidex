// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppMutationsGraphType : ObjectGraphType
    {
        public AppMutationsGraphType(IGraphModel model, IEnumerable<ISchemaEntity> schemas)
        {
            foreach (var schema in schemas)
            {
                var schemaId = schema.NamedId();
                var schemaType = schema.TypeName();
                var schemaName = schema.DisplayName();

                var contentType = model.GetContentType(schema.Id);

                var inputType = new ContentDataInputGraphType(schema, schemaName, schemaType, model);

                AddContentCreate(schemaId, schemaType, schemaName, inputType, contentType);
                AddContentUpdate(schemaType, schemaName, inputType, contentType);
                AddContentPatch(schemaType, schemaName, inputType, contentType);
                AddContentChangeStatus(schemaType, schemaName, contentType);
                AddContentDelete(schemaType, schemaName);
            }

            Description = "The app mutations.";
        }

        private void AddContentCreate(NamedId<Guid> schemaId, string schemaType, string schemaName, ContentDataInputGraphType inputType, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"create{schemaType}Content",
                Arguments = ContentArguments.Create(inputType, schemaName),
                ResolvedType = contentType,
                Resolver = ContentResolvers.Create(schemaId),
                Description = $"Creates an {schemaName} content."
            });
        }

        private void AddContentUpdate(string schemaType, string schemaName, ContentDataInputGraphType inputType, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"update{schemaType}Content",
                Arguments = ContentArguments.UpdateOrPatch(inputType, schemaName),
                ResolvedType = contentType,
                Resolver = ContentResolvers.Update,
                Description = $"Update an {schemaName} content by id."
            });
        }

        private void AddContentPatch(string schemaType, string schemaName, ContentDataInputGraphType inputType, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"patch{schemaType}Content",
                Arguments = ContentArguments.UpdateOrPatch(inputType, schemaName),
                ResolvedType = contentType,
                Resolver = ContentResolvers.Patch,
                Description = $"Patch an {schemaName} content by id."
            });
        }

        private void AddContentChangeStatus(string schemaType, string schemaName, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"publish{schemaType}Content",
                Arguments = ContentArguments.ChangeStatus(schemaName),
                ResolvedType = contentType,
                Resolver = ContentResolvers.ChangeStatus,
                Description = $"Publish a {schemaName} content."
            });
        }

        private void AddContentDelete(string schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"delete{schemaType}Content",
                Arguments = ContentArguments.Delete(schemaName),
                ResolvedType = EntitySavedGraphType.NonNull,
                Resolver = ContentResolvers.Delete,
                Description = $"Delete an {schemaName} content."
            });
        }
    }
}
