using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public class DefaultQueryProvider : IQueryProvider
    {
        private readonly IEnumerable<IQuery> queries;

        public DefaultQueryProvider(IEnumerable<IQuery> queries)
        {
            this.queries = queries;
        }

        public IEnumerable<IQuery> GetQueries(IAppEntity app, ISchemaEntity schema)
        {
            return queries.Where(query => query.AssociatedToApp == app.Name && query.AssociatedToSchema == schema.Name);
        }
    }
}
