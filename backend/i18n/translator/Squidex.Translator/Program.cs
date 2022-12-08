// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using CommandDotNet;
using CommandDotNet.FluentValidation;

namespace Squidex.Translator;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var appRunner =
                new AppRunner<Commands>()
                    .UseFluentValidation(true);

            return appRunner.Run(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: {0}", ex);
            return -1;
        }
    }
}
