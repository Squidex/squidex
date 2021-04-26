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
    public class DomainObjectException : DomainException
    {
        public string Id { get; }

        public DomainObjectException(string message, string id, string errorCode, Exception? inner = null)
            : base(message, errorCode, inner)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            Id = id;
        }

        public DomainObjectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Id = info.GetString(nameof(Id))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Id), Id);

            base.GetObjectData(info, context);
        }
    }
}
