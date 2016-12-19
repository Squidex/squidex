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
using System.Collections.Immutable;
// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Core.Schemas
{
    [TypeName("StringField")]
    public sealed class StringFieldProperties : FieldProperties
    {
        private int? minLength;
        private int? maxLength;
        private string pattern;
        private string patternMessage;
        private ImmutableList<string> allowedValues;
        private StringFieldEditor editor;

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

        public ImmutableList<string> AllowedValues
        {
            get { return allowedValues; }
            set
            {
                ThrowIfFrozen();

                allowedValues = value;
            }
        }

        public StringFieldEditor Editor
        {
            get { return editor; }
            set
            {
                ThrowIfFrozen();

                editor = value;
            }
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (!Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor ist not a valid value", nameof(Editor));
            }

            if ((Editor == StringFieldEditor.Radio || Editor == StringFieldEditor.Dropdown) && (AllowedValues == null || AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values", nameof(AllowedValues));
            }

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
