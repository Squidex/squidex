// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.States
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CollectionNameAttribute : Attribute
    {
        public string Name { get; }

        public CollectionNameAttribute(string name)
        {
            Name = name;
        }
    }
}
