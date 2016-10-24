// ==========================================================================
//  TypeNameAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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