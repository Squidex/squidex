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
        private readonly string id;
        private readonly string typeName;

        public string TypeName
        {
            get { return typeName; }
        }

        public string Id
        {
            get { return id; }
        }

        protected DomainObjectException(string message, string id, Type type, Exception inner = null)
            : base(message, inner)
        {
            this.id = id;

            typeName = type?.Name;
        }

        protected DomainObjectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            id = info.GetString(nameof(id));

            typeName = info.GetString(nameof(typeName));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(id), id);
            info.AddValue(nameof(typeName), typeName);

            base.GetObjectData(info, context);
        }
    }
}
