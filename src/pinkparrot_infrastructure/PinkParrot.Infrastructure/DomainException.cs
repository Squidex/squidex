// ==========================================================================
//  DomainException.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure
{
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
    }
}
