using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Types;
using NJsonSchema;

namespace Squidex.Domain.Apps.Read.Contents.CustomQueries
{
    public class QueryArgumentOption
    {
        public QueryArgumentOption(string name, string description, object defaultValue, QueryArgumentType argumentType)
        {
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
            ArgumentType = argumentType;
        }

        public string Name { get; }

        public string Description { get; }

        public object DefaultValue { get; }

        public QueryArgumentType ArgumentType { get; }

        public JsonObjectType ObjectType
        {
            get
            {
                JsonObjectType result = JsonObjectType.Null;

                switch (ArgumentType)
                {
                    case QueryArgumentType.Boolean:
                        result = JsonObjectType.Boolean;
                        break;
                    case QueryArgumentType.Number:
                        result = JsonObjectType.Number;
                        break;
                    case QueryArgumentType.String:
                        result = JsonObjectType.String;
                        break;
                }

                return result;
            }
        }
    }
}