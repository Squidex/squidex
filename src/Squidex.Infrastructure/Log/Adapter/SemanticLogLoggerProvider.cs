// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Log.Adapter
{
    public class SemanticLogLoggerProvider : ILoggerProvider
    {
        private readonly ISemanticLog semanticLog;

        public SemanticLogLoggerProvider(ISemanticLog semanticLog)
        {
            Guard.NotNull(semanticLog, nameof(semanticLog));

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
