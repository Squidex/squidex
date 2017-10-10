using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Sample.Plugin
{
    public class SamplePlugin : IQueryModule
    {
        private readonly IContentQueryService contentQuery;

        public SamplePlugin(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
        }

        public IEnumerable<IQuery> GetQueries(string appName, ISchemaEntity schema)
        {
            if (appName == "portal" && schema.Name == "node")
            {
                yield return new NewsPortalArticleTagQuery(this.contentQuery);
            }

            yield break;
        }
    }
}
