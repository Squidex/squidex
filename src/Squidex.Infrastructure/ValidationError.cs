// ==========================================================================
//  ValidationError.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure
{
    public sealed class ValidationError
    {
        private static readonly string[] FalbackProperties = new string[0];
        private readonly string message;
        private readonly string[] propertyNames;

        public string Message
        {
            get { return message; }
        }

        public IEnumerable<string> PropertyNames
        {
            get { return propertyNames; }
        }

        public ValidationError(string message, params string[] propertyNames)
        {
            Guard.NotNullOrEmpty(message, nameof(message));

            this.message = message;

            this.propertyNames = propertyNames ?? FalbackProperties;
        }
    }
}