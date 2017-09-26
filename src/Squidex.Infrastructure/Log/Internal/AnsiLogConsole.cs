// ==========================================================================
//  AnsiLogConsole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log.Internal
{
    public class AnsiLogConsole : IConsole
    {
        private readonly bool logToStdError;

        public AnsiLogConsole(bool logToStdError)
        {
            this.logToStdError = logToStdError;
        }

        public void WriteLine(bool isError, string message)
        {
            if (isError && logToStdError)
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
