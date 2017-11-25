// ==========================================================================
//  ConfigurationException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException()
        {
        }

        public ConfigurationException(string message)
            : base(message)
        {
        }

        public ConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
