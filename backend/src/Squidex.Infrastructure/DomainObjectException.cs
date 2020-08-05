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
    public class DomainObjectException : Exception
    {
        public string Id { get; }

        public DomainObjectException(string message, string id, Exception? inner = null)
            : base(message, inner)
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
