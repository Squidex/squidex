// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class UniqueConstraintException : Exception
    {
        public UniqueConstraintException()
        {
        }

        public UniqueConstraintException(string message)
            : base(message)
        {
        }

        public UniqueConstraintException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UniqueConstraintException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
