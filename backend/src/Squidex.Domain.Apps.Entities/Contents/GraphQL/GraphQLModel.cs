// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Log;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLModel
    {
        private static readonly IDocumentExecuter Executor = new DocumentExecuter();
        private readonly GraphQLSchema schema;
        private readonly ISemanticLog log;

        public GraphQLSchema Schema => schema;

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, SharedTypes typeFactory, ISemanticLog log)
        {
            this.log = log;

            schema = new Builder(app, typeFactory).BuildSchema(schemas);
        }

        public async Task<ExecutionResult> ExecuteAsync(ExecutionOptions options)
        {
            options.Schema = schema;

            var result = await Executor.ExecuteAsync(options);

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

            return result;
        }
    }
}
