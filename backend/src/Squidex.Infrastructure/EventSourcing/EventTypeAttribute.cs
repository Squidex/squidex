// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EventTypeAttribute(string typeName, int version = 1) : TypeNameAttribute(CreateTypeName(typeName, version))
{
    private const string Suffix = "Event";

    private static string CreateTypeName(string typeName, int version)
    {
        if (!typeName.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
        {
            typeName += Suffix;
        }

        if (version > 1)
        {
            typeName = $"{typeName}V{version}";
        }

        return typeName;
    }
}
