using System;
using System.Collections.Generic;
using System.Text;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface IQueryModulesService
    {
        IEnumerable<IQuery> GetQueriesFromAllQueryModules(string appName, ISchemaEntity schema);
    }
}
