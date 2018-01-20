// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TypeNameAttribute : Attribute
    {
        public string TypeName { get; }

        public TypeNameAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}