// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using ModelContextProtocol.Protocol;

namespace TestSuite.ApiTests.Settings;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        DerivePathInfo((sourceFile, projectDirectory, type, method) =>
        {
            var path = Path.Combine(projectDirectory, "Verify");

            return new PathInfo(path, type.Name, method.Name);
        });

        // The MCP metadata contains the server version and would make the snapshots brittle.
        VerifierSettings.ScrubMember<Result>(x => x.Meta);
    }
}
