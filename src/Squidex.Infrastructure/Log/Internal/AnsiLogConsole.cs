// ==========================================================================
//  AnsiLogConsole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text;

// ReSharper disable SwitchStatementMissingSomeCases

namespace Squidex.Infrastructure.Log.Internal
{
    public class AnsiLogConsole : IConsole
    {
        private readonly bool logToStdError;
        private readonly StringBuilder outputBuilder = new StringBuilder();

        public AnsiLogConsole(bool logToStdError)
        {
            this.logToStdError = logToStdError;
        }

        public void WriteLine(SemanticLogLevel level, string message)
        {
            if (level >= SemanticLogLevel.Error)
            {
                outputBuilder.Append("\x1B[1m\x1B[31m");
                outputBuilder.Append(message);
                outputBuilder.Append("\x1B[39m\x1B[22m");
                outputBuilder.AppendLine();

                if (logToStdError)
                {
                    Console.Error.Write(outputBuilder.ToString());
                }
                else
                {
                    Console.Out.WriteLine(outputBuilder.ToString());
                }

                outputBuilder.Clear();
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
