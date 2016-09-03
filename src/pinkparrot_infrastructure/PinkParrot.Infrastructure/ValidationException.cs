// ==========================================================================
//  ValidationException.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace PinkParrot.Infrastructure
{
    public class ValidationException : Exception
    {
        private readonly IReadOnlyList<string> errors;

        public IReadOnlyList<string> Errors
        {
            get { return errors; }
        }

        public ValidationException(string message, params string[] errors)
            : base(message)
        {
            this.errors = errors != null ? errors.ToList() : new List<string>();
        }

        public ValidationException(string message, IReadOnlyList<string> errors)
            : base(message)
        {
            this.errors = errors ?? new List<string>();
        }

        public ValidationException(string message, Exception inner, params string[] errors) 
            : base(message, inner)
        {
            this.errors = errors != null ? errors.ToList() : new List<string>();
        }

        public ValidationException(string message, Exception inner, IReadOnlyList<string> errors)
            : base(message, inner)
        {
            this.errors = errors ?? new List<string>();
        }
    }
}
