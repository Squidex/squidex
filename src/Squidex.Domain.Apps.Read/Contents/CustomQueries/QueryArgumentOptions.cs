using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Types;
using NJsonSchema;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public class QueryArgumentOptions
    {
        public QueryArgumentOptions(QueryArguments queryArguments)
        {
            GraphQlArguments = queryArguments;
            Arguments = new List<QueryArgumentOption>();

            foreach (var arg in GraphQlArguments)
            {
                var iarg = new QueryArgumentOption(arg.Name, arg.Description, arg.DefaultValue);
                iarg.SetObjectType(arg.ResolvedType);
                Arguments.Add(iarg);
            }
        }

        public QueryArguments GraphQlArguments { get; }

        public IList<QueryArgumentOption> Arguments { get; }
    }
}
