// ==========================================================================
//  DomainObjectException.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure
{
    public class DomainObjectException : Exception
    {
        private readonly string id;
        private readonly string typeName;

        public string TypeName
        {
            get
            {
                return typeName;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        protected DomainObjectException(string message, string id, Type type)
            : this(message, id, type, null)
        {
        }

        protected DomainObjectException(string message, string id, Type type, Exception inner)
            : base(message, inner)
        {
            this.id = id;

            if (type != null)
            {
                typeName = type.Name;
            }
        }
    }
}
