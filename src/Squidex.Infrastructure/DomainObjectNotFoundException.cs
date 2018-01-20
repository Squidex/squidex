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
    public class DomainObjectNotFoundException : DomainObjectException
    {
        public DomainObjectNotFoundException(string id, Type type)
            : base(FormatMessage(id, type), id, type)
        {
        }

        public DomainObjectNotFoundException(string id, string collection, Type type)
            : base(FormatMessage(id, collection, type), id, type)
        {
        }

        protected DomainObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string id, Type type)
        {
            return $"Domain object \'{id}\' (type {type}) is not found.";
        }

        private static string FormatMessage(string id, string collection, Type type)
        {
            return $"Domain object \'{id}\' not found on {type}.{collection}";
        }
    }
}
