// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLQuery
    {
        public string OperationName { get; set; }

        public string NamedQuery { get; set; }

        public string Query { get; set; }

        public JObject Variables { get; set; }
    }
}
