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
        public void WriteLine(SemanticLogLevel level, string message)
        {
            if (level >= SemanticLogLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.ResetColor();
            }
            else
            {
                Console.Out.WriteLine(message);
            }
        }
    }
}
