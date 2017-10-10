using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Sample.Plugin
{
    public class NewsPortalArticleTagQuery : IQuery
    {
        private readonly IContentQueryService contentQuery;

        public NewsPortalArticleTagQuery(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
            Arguments = new QueryArguments()
            {
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "tagIds",
                    Description = "The list of tag ids seperated by comma to filter by.",
                    DefaultValue = string.Empty
                }
            };
        }

        public string Name { get; } = "getWithTag";

        public string Description { get; } = "Sample query";

        public QueryArguments Arguments { get; }

        public Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> Execute(ISchemaEntity schema, QueryContext context, IDictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
