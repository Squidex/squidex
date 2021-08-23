// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Log;

namespace Squidex.Extensions.APM.Stackdriver
{
    public sealed class StackdriverSeverityLogAppender : ILogAppender
    {
        public void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception exception)
        {
            var severity = GetSeverity(logLevel);

            writer.WriteProperty(nameof(severity), severity);
        }

        private static string GetSeverity(SemanticLogLevel logLevel)
        {
            switch (logLevel)
            {
                case SemanticLogLevel.Trace:
                    return "DEBUG";
                case SemanticLogLevel.Debug:
                    return "DEBUG";
                case SemanticLogLevel.Information:
                    return "INFO";
                case SemanticLogLevel.Warning:
                    return "WARNING";
                case SemanticLogLevel.Error:
                    return "ERROR";
                case SemanticLogLevel.Fatal:
                    return "CRITICAL";
                default:
                    return "DEFAULT";
            }
        }
    }
}
