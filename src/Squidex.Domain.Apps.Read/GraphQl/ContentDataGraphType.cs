// ==========================================================================
//  SchemaGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.GraphQl
{
    public sealed class ContentDataGraphType : ObjectGraphType<NamedContentData>
    {
        private static readonly IFieldResolver FieldResolver = 
            new FuncFieldResolver<NamedContentData, ContentFieldData>(c => c.Source.GetOrDefault(c.FieldName));

        public ContentDataGraphType(Schema schema, IGraphQLContext context)
        {
            foreach (var field in schema.Fields)
            {
                var fieldName = field.Name;

                AddField(new FieldType
                {
                    Name = fieldName,
                    Resolver = FieldResolver,
                    ResolvedType = new SchemaFieldGraphType(field, context),
                });
            }
        }
    }
}
