// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;

#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.

namespace Squidex.Domain.Apps.Core;

public sealed class SquidexCoreModel
{
    public static readonly Assembly Assembly = typeof(SquidexCoreModel).Assembly;
}
