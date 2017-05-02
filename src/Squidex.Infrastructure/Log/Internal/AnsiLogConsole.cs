// ==========================================================================
//  AnsiLogConsole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

// ReSharper disable SwitchStatementMissingSomeCases

namespace Squidex.Infrastructure.Log.Internal
{
    public class AnsiLogConsole : IConsole
    {
        private readonly bool logToStdError;

        public AnsiLogConsole(bool logToStdError)
        {
            this.logToStdError = logToStdError;
        }

        public void WriteLine(SemanticLogLevel level, string message)
        {
            if (level >= SemanticLogLevel.Error && logToStdError)
            {
                Console.Error.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
