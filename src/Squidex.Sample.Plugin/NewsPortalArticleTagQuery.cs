using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Extensibility;

namespace Squidex.Sample.Plugin
{
    public class NewsPortalArticleTagQuery : IQuery
    {
        private readonly IContentQueryService contentQuery;

        public NewsPortalArticleTagQuery(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
        }

        public string Name { get; } = "getWithTag";

        public Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> Execute(QueryContext context, object[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
