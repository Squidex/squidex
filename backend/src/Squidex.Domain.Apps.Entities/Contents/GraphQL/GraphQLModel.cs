// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Log;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLModel
    {
        private static readonly IDocumentExecuter Executor = new DocumentExecuter();
        private readonly GraphQLSchema schema;
        private readonly ISemanticLog log;

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, SharedTypes typeFactory, ISemanticLog log)
        {
            this.log = log;

            schema = new Builder(app, typeFactory).BuildSchema(schemas);
        }

        public async Task<(object Data, object[]? Errors)> ExecuteAsync(GraphQLExecutionContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));

            var result = await Executor.ExecuteAsync(execution =>
            {
                context.Setup(execution);

                execution.Schema = schema;
                execution.Inputs = query.Inputs;
                execution.Query = query.Query;
            });

            if (result.Errors != null && result.Errors.Any())
            {
                log.LogWarning(w => w
                    .WriteProperty("action", "GraphQL")
                    .WriteProperty("status", "Failed")
                    .WriteArray("errors", a =>
                    {
                        foreach (var error in result.Errors)
                        {
                            a.WriteObject(error, (error, e) => e.WriteException(error));
                        }
                    }));
            }

            var errors = result.Errors?.Select(x => (object)new { x.Message, x.Locations }).ToArray();

            return (result.Data, errors);
        }
    }
}
