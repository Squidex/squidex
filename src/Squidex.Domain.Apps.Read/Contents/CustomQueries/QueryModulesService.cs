using System;
using System.Collections.Generic;
using System.Text;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public class QueryModulesService : IQueryModulesService
    {
        private readonly IEnumerable<IQueryModule> plugins;

        public QueryModulesService(IEnumerable<IQueryModule> plugins)
        {
            this.plugins = plugins;
        }

        public IEnumerable<IQuery> GetQueriesFromAllQueryModules(string appName, ISchemaEntity schema)
        {
            foreach (var plugin in plugins)
            {
                var queries = plugin.GetQueries(appName, schema);
                foreach (var q in queries)
                {
                    yield return q;
                }
            }
        }
    }
}
