using System.Collections.Generic;
using System.Threading.Tasks;
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

            var args = new List<QueryArgumentOption>()
            {
                new QueryArgumentOption("tagNames", "The list of tag names seperated by comma to filter by.", string.Empty, QueryArgumentType.String)
            };

            this.ArgumentOptions = args;
        }

        public string Name { get; } = "getNodesWithTag";

        public string Summary { get; } = "Get nodes with a specific tag";

        public string DescriptionForSwagger { get; } = string.Empty;

        public string AssociatedToApp { get; } = "portal";

        public string AssociatedToSchema { get; } = "node";

        public IList<QueryArgumentOption> ArgumentOptions { get; }

        public async Task<IReadOnlyList<IContentEntity>> Execute(ISchemaEntity schema, QueryContext context, IDictionary<string, object> arguments)
        {
            if (arguments == null || arguments.Count == 0 || !arguments.ContainsKey("tagNames"))
            {
                return new List<IContentEntity>();
            }

            var filterList = new List<string>();
            var parts = arguments["tagNames"].ToString().Split(",");
            foreach (var part in parts)
            {
                filterList.Add($"data/title/en eq '{part}'");
            }

            var filter = string.Join(" or ", filterList);

            var terms = await context.QueryContentsAsync("term", $"$filter={filter}");
            filterList.Clear();

            foreach (var term in terms)
            {
                filterList.Add($"data/terms/iv eq '{term.Id}'");
            }

            filter = string.Join(" or ", filterList);
            var resultContents = await context.QueryContentsAsync(schema.Name, $"$filter={filter}");

            return resultContents;
        }
    }
}
