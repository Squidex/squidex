// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;

namespace Squidex.Domain.Apps.Core.TestHelpers;

/// <summary>
/// Turns on Jint's host-contract verifiers for this test assembly.
/// </summary>
/// <remarks>
/// The scripting integration defines several Jint extension points - the ContentWrapper objects override
/// GetOwnProperty and ProbeOwnProperty, and the engine trusts both without re-verifying them on the hot
/// path. A hook that contradicts another therefore fails silently in production: a key vanishes from every
/// enumeration, or a read resolves on the prototype for a property that exists. With the switch on, Jint
/// recomputes the answer the fast paths exist to avoid and throws on the first disagreement, so these tests
/// are the checker.
/// <para>
/// It has to be set before the first use of any Jint type - the flag is read once at type initialization -
/// which is what the module initializer guarantees. Never turn it on in production: the verifiers
/// deliberately redo the work they check.
/// </para>
/// </remarks>
internal static class JintHostContractVerification
{
    [ModuleInitializer]
    internal static void Enable()
    {
        AppContext.SetSwitch("Jint.EnableHostContractVerification", true);
    }
}
