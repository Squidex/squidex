// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Log.Internal
{
    [ExcludeFromCodeCoverage]
    public sealed class WindowsLogConsole : IConsole
    {
        private readonly bool logToStdError;

        public WindowsLogConsole(bool logToStdError)
        {
            this.logToStdError = logToStdError;
        }

        public void Reset()
        {
            Console.ResetColor();
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
