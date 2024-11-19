// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FieldDescriptionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
