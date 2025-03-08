// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.EntityFramework.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ReuseLabelAttribute(string label) : Attribute
{
    public string Label { get; } = label;
}
