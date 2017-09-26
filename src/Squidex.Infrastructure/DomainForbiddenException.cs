// ==========================================================================
//  DomainForbiddenException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
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
    }
}
