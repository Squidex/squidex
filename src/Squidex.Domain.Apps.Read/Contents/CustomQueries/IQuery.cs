using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface IQuery
    {
        string Name { get; }

        string Description { get; }

        Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> Execute(QueryContext context,
            object[] arguments);
    }
}
