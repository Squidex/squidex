using Squidex.Domain.Apps.Read.Schemas;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Contents;

namespace Squidex.Extensibility
{
    public interface IQuery
    {
        string Name { get; }

        Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> Execute(QueryContext context,
            object[] arguments);
    }
}
