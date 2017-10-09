using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface IQueryModule
    {
        IEnumerable<IQuery> GetQueries(string appName, ISchemaEntity schema);
    }
}
