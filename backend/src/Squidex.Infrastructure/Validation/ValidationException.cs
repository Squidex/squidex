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
using System.Text;

namespace Squidex.Infrastructure.Validation
{
    [Serializable]
    public class ValidationException : DomainException
    {
        private readonly IReadOnlyList<ValidationError> errors;

        public IReadOnlyList<ValidationError> Errors
        {
            get { return errors; }
        }

        public ValidationException(string error, Exception? inner = null)
            : this(new List<ValidationError> { new ValidationError(error) }, inner)
        {
        }

        public ValidationException(ValidationError error, Exception? inner = null)
            : this(new List<ValidationError> { error }, inner)
        {
        }

        public ValidationException(IReadOnlyList<ValidationError> errors, Exception? inner = null)
            : base(FormatMessage(errors), inner)
        {
            Guard.NotEmpty(errors, nameof(errors));

            this.errors = errors;
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            errors = (List<ValidationError>)info.GetValue(nameof(errors), typeof(List<ValidationError>))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(errors), errors.ToList());

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(IReadOnlyList<ValidationError>? errors)
        {
            var sb = new StringBuilder();

            if (errors?.Count > 0)
            {
                sb.Append(": ");

                for (var i = 0; i < errors.Count; i++)
                {
                    var error = errors[i]?.Message;

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        sb.Append(error);

                        if (!error.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(".");
                        }

                        if (i < errors.Count - 1)
                        {
                            sb.Append(" ");
                        }
                    }
                }
            }
            else
            {
                sb.Append(".");
            }

            return sb.ToString();
        }
    }
}
