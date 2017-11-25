// ==========================================================================
//  DomainForbiddenException.cs
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
    public class DomainForbiddenException : DomainException
    {
        public DomainForbiddenException(string message)
            : base(message)
        {
        }

        public DomainForbiddenException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DomainForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
