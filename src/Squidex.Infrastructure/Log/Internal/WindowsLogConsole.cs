// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public void WriteLine(int color, string message)
        {
            if (color != 0)
            {
                try
                {
                    if (color == 0xffff00)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }

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
