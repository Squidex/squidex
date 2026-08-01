// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;

namespace Squidex.Domain.Apps.Core.TestHelpers;

/// <summary>
/// Turns on Jint's self checks for this test assembly.
/// </summary>
/// <remarks>
/// Our ContentWrapper classes and the object converter implement Jint extension points where Jint relies on
/// our answers being consistent, without checking them - checking would cost as much as the shortcut saves.
/// A mistake there is silent in production: a key can disappear from Object.keys, or a converted type can be
/// skipped. With this switch on Jint verifies the answers and throws on the first mismatch, so a mistake
/// fails a test instead.
/// <para>
/// The switch has to be set before the first Jint type is used, which is what the module initializer
/// guarantees. It stays off in production, where the checks would only cost performance.
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
