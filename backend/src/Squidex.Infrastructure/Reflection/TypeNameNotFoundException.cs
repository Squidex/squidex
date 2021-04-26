// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.Reflection
{
    [Serializable]
    public class TypeNameNotFoundException : Exception
    {
        public TypeNameNotFoundException(string? message = null, Exception? inner = null)
            : base(message, inner)
        {
        }

        protected TypeNameNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
