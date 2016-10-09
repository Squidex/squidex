// ==========================================================================
//  TypeNameAttribute.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure
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