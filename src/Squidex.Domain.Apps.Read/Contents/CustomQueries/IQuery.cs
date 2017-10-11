using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public interface IQuery
    {
        string Name { get; }

        string Summary { get; }

        string DescriptionForSwagger { get; }

        QueryArgumentOptions ArgumentOptions { get; }

        Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> Execute(ISchemaEntity schema, QueryContext context,
            IDictionary<string, object> arguments);
    }
}
