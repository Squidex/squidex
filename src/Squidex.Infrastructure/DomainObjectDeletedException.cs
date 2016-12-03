// ==========================================================================
//  DomainObjectDeletedException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public class DomainObjectDeletedException : DomainObjectException
    {
        public DomainObjectDeletedException(string id, Type type)
            : base(FormatMessage(id, type), id, type)
        {
        }

        private static string FormatMessage(string id, Type type)
        {
            return $"Domain object \'{id}\' (type {type}) already deleted.";
        }
    }
}
