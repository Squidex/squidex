// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Reflection;

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
