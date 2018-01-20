// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLModel : IGraphModel
    {
        private readonly Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>> fieldInfos;
        private readonly Dictionary<Type, IGraphType> inputFieldInfos;
        private readonly Dictionary<ISchemaEntity, ContentGraphType> contentTypes = new Dictionary<ISchemaEntity, ContentGraphType>();
        private readonly Dictionary<ISchemaEntity, ContentDataGraphType> contentDataTypes = new Dictionary<ISchemaEntity, ContentDataGraphType>();
        private readonly Dictionary<Guid, ISchemaEntity> schemas;
        private readonly PartitionResolver partitionResolver;
        private readonly IAppEntity app;
        private readonly IGraphType assetListType;
        private readonly IComplexGraphType assetType;
        private readonly GraphQLSchema graphQLSchema;

        public bool CanGenerateAssetSourceUrl { get; }

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, IGraphQLUrlGenerator urlGenerator)
        {
            this.app = app;

            CanGenerateAssetSourceUrl = urlGenerator.CanGenerateAssetSourceUrl;

            partitionResolver = app.PartitionResolver();

            assetType = new AssetGraphType(this);
            assetListType = new ListGraphType(new NonNullGraphType(assetType));

            inputFieldInfos = new Dictionary<Type, IGraphType>
            {
                {
                    typeof(StringField),
                    AllTypes.String
                },
                {
                    typeof(BooleanField),
                    AllTypes.Boolean
                },
                {
                    typeof(NumberField),
                    AllTypes.Boolean
                },
                {
                    typeof(DateTimeField),
                    AllTypes.Date
                },
                {
                    typeof(GeolocationField),
                    AllTypes.GeolocationInput
                },
                {
                    typeof(TagsField),
                    AllTypes.ListOfNonNullString
                },
                {
                    typeof(AssetsField),
                    AllTypes.ListOfNonNullGuid
                },
                {
                    typeof(ReferencesField),
                    AllTypes.ListOfNonNullGuid
                }
            };

            fieldInfos = new Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>>
            {
                {
                    typeof(StringField),
                    field => ResolveDefault(AllTypes.NoopString)
                },
                {
                    typeof(BooleanField),
                    field => ResolveDefault(AllTypes.NoopBoolean)
                },
                {
                    typeof(NumberField),
                    field => ResolveDefault(AllTypes.NoopFloat)
                },
                {
                    typeof(DateTimeField),
                    field => ResolveDefault(AllTypes.NoopDate)
                },
                {
                    typeof(JsonField),
                    field => ResolveDefault(AllTypes.NoopJson)
                },
                {
                    typeof(GeolocationField),
                    field => ResolveDefault(AllTypes.NoopGeolocation)
                },
                {
                    typeof(TagsField),
                    field => ResolveDefault(AllTypes.NoopTags)
                },
                {
                    typeof(AssetsField),
                    field => ResolveAssets(assetListType)
                },
                {
                    typeof(ReferencesField),
                    field => ResolveReferences(field)
                }
            };

            this.schemas = schemas.ToDictionary(x => x.Id);

            var m = new AppMutationsGraphType(this, this.schemas.Values);
            var q = new AppQueriesGraphType(this, this.schemas.Values);

            graphQLSchema = new GraphQLSchema { Query = q, Mutation = m };

            foreach (var kvp in contentDataTypes)
            {
                kvp.Value.Initialize(this, kvp.Key);
            }

            foreach (var kvp in contentTypes)
            {
                kvp.Value.Initialize(this, kvp.Key, contentDataTypes[kvp.Key]);
            }
        }

        private static (IGraphType ResolveType, IFieldResolver Resolver) ResolveDefault(IGraphType type)
        {
            return (type, new FuncFieldResolver<ContentFieldData, object>(c => c.Source.GetOrDefault(c.FieldName)));
        }

        public IFieldResolver ResolveAssetUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveAssetSourceUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetSourceUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveAssetThumbnailUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetThumbnailUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveContentUrl(ISchemaEntity schema)
        {
            var resolver = new FuncFieldResolver<IContentEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateContentUrl(app, schema, c.Source);
            });

            return resolver;
        }

        private static ValueTuple<IGraphType, IFieldResolver> ResolveAssets(IGraphType assetListType)
        {
            var resolver = new FuncFieldResolver<ContentFieldData, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;
                var contentIds = c.Source.GetOrDefault(c.FieldName);

                return context.GetReferencedAssetsAsync(contentIds);
            });

            return (assetListType, resolver);
        }

        private ValueTuple<IGraphType, IFieldResolver> ResolveReferences(Field field)
        {
            var schemaId = ((ReferencesField)field).Properties.SchemaId;

            var contentType = GetContentType(schemaId);

            if (contentType == null)
            {
                return (null, null);
            }

            var resolver = new FuncFieldResolver<ContentFieldData, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;
                var contentIds = c.Source.GetOrDefault(c.FieldName);

                return context.GetReferencedContentsAsync(schemaId, contentIds);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(contentType));

            return (schemaFieldType, resolver);
        }

        public async Task<(object Data, object[] Errors)> ExecuteAsync(GraphQLExecutionContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));

            var result = await new DocumentExecuter().ExecuteAsync(options =>
            {
                options.Query = query.Query;
                options.Schema = graphQLSchema;
                options.Inputs = query.Variables?.ToInputs() ?? new Inputs();
                options.UserContext = context;
                options.OperationName = query.OperationName;
            }).ConfigureAwait(false);

            return (result.Data, result.Errors?.Select(x => (object)new { x.Message, x.Locations }).ToArray());
        }

        public IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public IComplexGraphType GetAssetType()
        {
            return assetType;
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) GetGraphType(Field field)
        {
            return fieldInfos[field.GetType()](field);
        }

        public IComplexGraphType GetContentDataType(Guid schemaId)
        {
            var schema = schemas.GetOrDefault(schemaId);

            if (schema == null)
            {
                return null;
            }

            return schema != null ? contentDataTypes.GetOrAdd(schema, s => new ContentDataGraphType()) : null;
        }

        public IComplexGraphType GetContentType(Guid schemaId)
        {
            var schema = schemas.GetOrDefault(schemaId);

            if (schema == null)
            {
                return null;
            }

            return contentTypes.GetOrAdd(schema, s => new ContentGraphType());
        }

        public IGraphType GetInputGraphType(Field field)
        {
            return inputFieldInfos.GetOrAddDefault(field.GetType());
        }
    }
}
