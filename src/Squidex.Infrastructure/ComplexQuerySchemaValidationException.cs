using System;

namespace Squidex.Infrastructure
{
    public class ComplexQuerySchemaValidationException : Exception
    {
        public string QueryName { get; }

        public ComplexQuerySchemaValidationException(string message, string queryName)
            : base(message)
        {
            QueryName = queryName;
        }
    }
}
