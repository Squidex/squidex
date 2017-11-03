// ==========================================================================
//  GreaterQuery.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public sealed class GreaterQuery : ICustomQueryProvider, ICustomQuery
    {
        public string Name { get; } = "great";

        public string Summary { get; } = "Says Hello";

        public string Description { get; } = "Says Hello";

        public IReadOnlyList<QueryArgumentOption> ArgumentOptions { get; }

        public GreaterQuery()
        {
            ArgumentOptions = new List<QueryArgumentOption>
            {
                new QueryArgumentOption("user", "The name of the user to say hello to.")
            };
        }

        public Task<JToken> ExecuteAsync(QueryContext context, IDictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("user", out var user))
            {
                user = "unknown";
            }

            var result = new { message = $"Hello {user}." };

            return Task.FromResult(JToken.FromObject(result));
        }

        public Task<IReadOnlyList<ICustomQuery>> GetQueriesAsync(IAppEntity app)
        {
            return Task.FromResult<IReadOnlyList<ICustomQuery>>(new List<ICustomQuery> { this });
        }
    }
}
