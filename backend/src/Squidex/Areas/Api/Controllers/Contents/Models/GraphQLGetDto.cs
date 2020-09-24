// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.NewtonsoftJson;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public class GraphQLGetDto
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public string Variables { get; set; }

        public GraphQLQuery ToQuery()
        {
            var query = SimpleMapper.Map(this, new GraphQLQuery());

            query.Inputs = Variables?.ToInputs();

            return query;
        }
    }
}
