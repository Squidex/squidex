using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Types;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;

namespace Squidex.Sample.Plugin
{
    public class NewsPortalArticleTagQuery : IQuery
    {
        private readonly IContentQueryService contentQuery;
        private readonly ISchemaProvider m_schemas;

        public NewsPortalArticleTagQuery(IContentQueryService contentQuery, ISchemaProvider schemas)
        {
            this.contentQuery = contentQuery;
            m_schemas = schemas;
            var args = new QueryArguments()
            {
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "tagNames",
                    Description = "The list of tag names seperated by comma to filter by.",
                    DefaultValue = string.Empty
                }
            };

            this.ArgumentOptions = new QueryArgumentOptions(args);
        }

        public string Name { get; } = "getNodesWithTag";

        public string Summary { get; } = "Get nodes with a specific tag";

        public string DescriptionForSwagger { get; } = string.Empty;

        public string AssociatedToApp { get; } = "portal";

        public string AssociatedToSchema { get; } = "node";

        public QueryArgumentOptions ArgumentOptions { get; }

        public async Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> Execute(ISchemaEntity schema, QueryContext context, IDictionary<string, object> arguments)
        {
            if (arguments == null || arguments.Count == 0 || !arguments.ContainsKey("tagNames"))
            {
                return (schema, 0, new List<IContentEntity>());
            }

            var filterList = new List<string>();
            var parts = arguments["tagNames"].ToString().Split(",");
            foreach (var part in parts)
            {
                filterList.Add($"data/title/en eq '{part}'");
            }

            var filter = string.Join(" or ", filterList);

            // var terms = await context.QueryContentsAsync("term", $"$filter={filter}");
            var resultContents = await context.QueryContentsAsync("term", $"$filter={filter}");
            var termSchema = await m_schemas.FindSchemaByNameAsync(schema.AppId, "term");
            //filterList.Clear();

            //foreach (var term in terms)
            //{
            //    filterList.Add($"data/terms/iv eq '{term.Id}'");
            //}

            //filter = string.Join(" or ", filterList);
            //var resultContents = await context.QueryContentsAsync(schema.Name, $"$filter={filter}");

            return (termSchema, resultContents.Count, resultContents);
        }
    }
}
