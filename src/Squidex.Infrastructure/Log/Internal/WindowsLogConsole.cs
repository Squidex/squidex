// ==========================================================================
//  WindowsLogConsole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log.Internal
{
    public class WindowsLogConsole : IConsole
    {
        private readonly bool logToStdError;

        public WindowsLogConsole(bool logToStdError)
        {
            this.logToStdError = logToStdError;
        }

        public void WriteLine(bool isError, string message)
        {
            if (isError)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    if (logToStdError)
                    {
                        Console.Error.WriteLine(message);
                    }
                    else
                    {
                        Console.Out.WriteLine(message);
                    }
                }
                finally
                {
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
