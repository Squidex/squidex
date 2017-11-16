// ==========================================================================
//  OrleansException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.Orleans
{
    [Serializable]
    public class OrleansException : Exception
    {
        public OrleansException()
        {
        }

        public OrleansException(string message)
            : base(message)
        {
        }

        public OrleansException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected OrleansException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
