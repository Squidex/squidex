// ==========================================================================
//  StringFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Squidex.Infrastructure;
// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Core.Schemas
{
    public sealed class StringFieldProperties : FieldProperties
    {
        private int? minLength;
        private int? maxLength;
        private string pattern;
        private string patternMessage;

        public int? MinLength
        {
            get { return minLength; }
            set
            {
                ThrowIfFrozen();

                minLength = value;
            }
        }

        public int? MaxLength
        {
            get { return maxLength; }
            set
            {
                ThrowIfFrozen();

                maxLength = value;
            }
        }

        public string Pattern
        {
            get { return pattern; }
            set
            {
                ThrowIfFrozen();

                pattern = value;
            }
        }

        public string PatternMessage
        {
            get { return patternMessage; }
            set
            {
                ThrowIfFrozen();

                patternMessage = value;
            }
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (MaxLength.HasValue && MinLength.HasValue && MinLength.Value >= MaxLength.Value)
            {
                yield return new ValidationError("Max length must be greater than min length", nameof(MinLength), nameof(MaxLength));
            }

            if (Pattern == null)
            {
                yield break;
            }

            var isValidPattern = true;

            try
            {
                new Regex(Pattern);
            }
            catch (ArgumentException)
            {
                isValidPattern = false;
            }

            if (!isValidPattern)
            {
                yield return new ValidationError("Pattern is not a valid expression", nameof(Pattern));
            }
        }
    }
}
