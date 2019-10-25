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
        public string? TypeName { get; }

        public string Id { get; }

        protected DomainObjectException(string message, string id, Type type, Exception? inner = null)
            : base(message, inner)
        {
            Id = id;

            TypeName = type?.Name;
        }

        protected DomainObjectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Id = info.GetString(nameof(Id))!;

            TypeName = info.GetString(nameof(TypeName))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(TypeName), TypeName);

            base.GetObjectData(info, context);
        }
    }
}
