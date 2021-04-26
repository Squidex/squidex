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
    public class DomainException : Exception
    {
        public string? ErrorCode { get; }

        public DomainException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }

        public DomainException(string message, string? errorCode, Exception? inner = null)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }

        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ErrorCode), ErrorCode);

            base.GetObjectData(info, context);
        }
    }
}
