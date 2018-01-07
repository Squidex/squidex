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

        protected TypeNameNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
