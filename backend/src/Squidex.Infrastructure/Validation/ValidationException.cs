// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Squidex.Infrastructure.Validation
{
    [Serializable]
    public class ValidationException : DomainException
    {
        private const string ValidationError = "VALIDATION_ERROR";

        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationException(string error, Exception? inner = null)
            : this(new ValidationError(error), inner)
        {
        }

        public ValidationException(ValidationError error, Exception? inner = null)
            : this(new List<ValidationError> { error }, inner)
        {
        }

        public ValidationException(IReadOnlyList<ValidationError> errors, Exception? inner = null)
            : base(FormatMessage(errors), ValidationError, inner)
        {
            Errors = errors;
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Errors = (List<ValidationError>)info.GetValue(nameof(Errors), typeof(List<ValidationError>))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Errors), Errors);

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(IReadOnlyList<ValidationError> errors)
        {
            Guard.NotNull(errors, nameof(errors));

            var sb = new StringBuilder();

            for (var i = 0; i < errors.Count; i++)
            {
                var error = errors[i]?.Message;

                if (!string.IsNullOrWhiteSpace(error))
                {
                    sb.Append(error);

                    if (!error.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('.');
                    }

                    if (i < errors.Count - 1)
                    {
                        sb.Append(' ');
                    }
                }
            }

            return sb.ToString();
        }
    }
}
