// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public void Reset()
        {
        }

        public void WriteLine(int color, string message)
        {
            if (color != 0 && logToStdError)
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
