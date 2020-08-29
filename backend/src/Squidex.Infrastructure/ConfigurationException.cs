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
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
