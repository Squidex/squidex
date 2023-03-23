﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;

namespace TestSuite.ApiTests.Settings;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Verifier.DerivePathInfo((sourceFile, projectDirectory, type, method) =>
        {
            var path = Path.Combine(projectDirectory, "Verify");

            return new PathInfo(path, type.Name, method.Name);
        });
    }
}
