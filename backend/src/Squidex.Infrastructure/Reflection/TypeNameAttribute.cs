// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Reflection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TypeNameAttribute(string typeName) : Attribute
{
    public string TypeName { get; } = typeName;
}
