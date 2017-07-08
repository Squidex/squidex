using System;
using GraphQL.Types;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.GraphQl
{
    public interface IGraphQLResolver
    {
        IGraphType GetSchemaListType(Schema schema);

        IGraphType GetSchemaListType(Guid schemaId);

        IGraphType GetAssetListType();
    }
}