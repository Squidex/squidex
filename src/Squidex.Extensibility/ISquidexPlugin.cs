using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Extensibility
{
    public interface ISquidexPlugin
    {
        IEnumerable<IQuery> GetQueries(IAppEntity app, ISchemaEntity schema);
    }
}
