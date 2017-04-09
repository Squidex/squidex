// ==========================================================================
//  SemanticLogLoggerProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Log.Adapter
{
    public class SemanticLogLoggerProvider : ILoggerProvider
    {
        private readonly ISemanticLog semanticLog;

        public SemanticLogLoggerProvider(ISemanticLog semanticLog)
        {
            this.semanticLog = semanticLog;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SemanticLogLogger(semanticLog.CreateScope(writer =>
            {
                writer.WriteProperty("category", categoryName);
            }));
        }

        public void Dispose()
        {
        }
    }
}
