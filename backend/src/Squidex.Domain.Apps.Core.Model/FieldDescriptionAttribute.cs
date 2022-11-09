// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FieldDescriptionAttribute : Attribute
{
    public string Name { get; }

    public FieldDescriptionAttribute(string name)
    {
        Name = name;
    }
}
