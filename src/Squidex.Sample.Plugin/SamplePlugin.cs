using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Extensibility;

namespace Squidex.Sample.Plugin
{
    public class SamplePlugin : ISquidexPlugin
    {
        private readonly IContentQueryService contentQuery;

        public SamplePlugin(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
        }

        public IEnumerable<IQuery> GetQueries(IAppEntity app, ISchemaEntity schema)
        {
            if (app.Name == "portal" && schema.Name == "news")
            {
                yield return new NewsPortalArticleTagQuery(this.contentQuery);
            }

            yield break;
        }
    }
}
