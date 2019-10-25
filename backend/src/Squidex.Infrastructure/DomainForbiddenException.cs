// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
