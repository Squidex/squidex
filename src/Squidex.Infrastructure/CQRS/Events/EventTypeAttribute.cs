// ==========================================================================
//  EventTypeAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EventTypeAttribute : TypeNameAttribute
    {
        public EventTypeAttribute(string typeName, int version = 1)
            : base(CreateTypeName(typeName, version))
        {
        }

        private static string CreateTypeName(string typeName, int version)
        {
            return $"{typeName}Event" + (version > 1 ? $"V{version}" : string.Empty);
        }
    }
}
