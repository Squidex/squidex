// ==========================================================================
//  TypeNameNotFoundException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public class TypeNameNotFoundException : Exception
    {
        public TypeNameNotFoundException()
        {
        }

        public TypeNameNotFoundException(string message)
            : base(message)
        {
        }

        public TypeNameNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
