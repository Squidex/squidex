// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class DomainObjectNotFoundException : DomainObjectException
    {
        private const string ValidationError = "OBJECT_NOTFOUND";

        public DomainObjectNotFoundException(string id, Exception? inner = null)
            : base(FormatMessage(id), id, ValidationError, inner)
        {
        }

        protected DomainObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string id)
        {
            return T.Get("exceptions.domainObjectNotFound", new { id });
        }
    }
}
