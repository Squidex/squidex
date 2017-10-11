using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Types;
using NJsonSchema;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public class QueryArgumentOption
    {
        public QueryArgumentOption(string name, string description, object defaultValue)
        {
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public string Description { get; }

        public object DefaultValue { get; }

        public JsonObjectType ObjectType { get; private set; }

        public void SetObjectType(IGraphType type)
        {
            if (type is StringGraphType)
            {
                ObjectType = JsonObjectType.String;
            }

            if (type is IntGraphType)
            {
                ObjectType = JsonObjectType.Integer;
            }

            if (type is BooleanGraphType)
            {
                ObjectType = JsonObjectType.Boolean;
            }
        }
    }
}
