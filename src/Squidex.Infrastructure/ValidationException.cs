// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure
{
    [Serializable]
    public class ValidationException : Exception
    {
        private static readonly List<ValidationError> FallbackErrors = new List<ValidationError>();
        private readonly IReadOnlyList<ValidationError> errors;

        public IReadOnlyList<ValidationError> Errors
        {
            get { return errors; }
        }

        public ValidationException(string message, params ValidationError[] errors)
            : base(message)
        {
            this.errors = errors != null ? errors.ToList() : FallbackErrors;
        }

        public ValidationException(string message, IReadOnlyList<ValidationError> errors)
            : base(message)
        {
            this.errors = errors ?? FallbackErrors;
        }

        public ValidationException(string message, Exception inner, params ValidationError[] errors)
            : base(message, inner)
        {
            this.errors = errors != null ? errors.ToList() : FallbackErrors;
        }

        public ValidationException(string message, Exception inner, IReadOnlyList<ValidationError> errors)
            : base(message, inner)
        {
            this.errors = errors ?? FallbackErrors;
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            return string.Join(" ", Enumerable.Repeat(Message, 1).Union(Errors.Select(x => x.Message)));
        }
    }
}
