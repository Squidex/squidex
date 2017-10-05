using System;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Extensibility
{
    public class QueryArgument
    {
        public QueryArgument(string name, string description, Type argumentType)
        {
            Name = name;
            Description = description;
            ArgumentType = argumentType;
        }

        public string Name { get; }

        public string Description { get; }

        public Type ArgumentType { get; }


    }
}
