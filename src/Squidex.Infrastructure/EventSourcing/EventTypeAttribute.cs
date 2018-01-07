// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
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
