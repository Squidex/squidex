﻿// ==========================================================================
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
        private static readonly List<ValidationError> FallbackErrors = new List<ValidationError>();
        private readonly IReadOnlyList<ValidationError> errors;

        public IReadOnlyList<ValidationError> Errors
        {
            get { return errors ?? FallbackErrors; }
        }

        public string Summary { get; }

        public ValidationException(string summary, params ValidationError[]? errors)
            : this(summary, errors?.ToList())
        {
        }

        public ValidationException(string summary, IReadOnlyList<ValidationError>? errors)
            : this(summary, null, errors)
        {
        }

        public ValidationException(string summary, Exception? inner, params ValidationError[]? errors)
            : this(summary, inner, errors?.ToList())
        {
        }

        public ValidationException(string summary, Exception? inner, IReadOnlyList<ValidationError>? errors)
            : base(FormatMessage(summary, errors), inner!)
        {
            Summary = summary;

            this.errors = errors ?? FallbackErrors;
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Summary = info.GetString(nameof(Summary))!;

            errors = (List<ValidationError>)info.GetValue(nameof(errors), typeof(List<ValidationError>))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Summary), Summary);
            info.AddValue(nameof(errors), errors.ToList());

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(string summary, IReadOnlyList<ValidationError>? errors)
        {
            var sb = new StringBuilder();

            sb.Append(summary.TrimEnd(' ', '.', ':'));

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
