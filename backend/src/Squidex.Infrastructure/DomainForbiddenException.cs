// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class DomainForbiddenException : DomainException
    {
        private const string ValidationError = "FORBIDDEN";

        public DomainForbiddenException(string message, Exception? inner = null)
            : base(message, ValidationError, inner)
        {
        }

        protected DomainForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
