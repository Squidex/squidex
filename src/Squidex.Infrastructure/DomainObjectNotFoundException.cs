// ==========================================================================
//  DomainObjectNotFoundException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public class DomainObjectNotFoundException : DomainObjectException
    {
        public DomainObjectNotFoundException(string id, Type type)
            : base(FormatMessage(id, type), id, type)
        {
        }

        private static string FormatMessage(string id, Type type)
        {
            return $"Domain object \'{id}\' (type {type}) not found.";
        }
    }
}
