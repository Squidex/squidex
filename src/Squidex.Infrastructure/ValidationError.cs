// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure
{
    public sealed class ValidationError
    {
        private static readonly string[] FallbackProperties = new string[0];
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

            this.propertyNames = propertyNames ?? FallbackProperties;
        }

        public ValidationError WithPrefix(string prefix)
        {
            if (propertyNames.Length > 0)
            {
                return new ValidationError(Message, propertyNames.Select(x => $"{prefix}.{x}").ToArray());
            }
            else
            {
                return new ValidationError(Message, prefix);
            }
        }

        public void AddTo(AddValidation e)
        {
            e(Message, propertyNames);
        }
    }
}
