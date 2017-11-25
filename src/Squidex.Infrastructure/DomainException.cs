// ==========================================================================
//  DomainException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
