// ==========================================================================
//  FieldProperties.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schemas
{
    public abstract class FieldProperties : NamedElementProperties, IFieldProperties
    {
        public bool IsRequired { get; }

        protected FieldProperties(string label,  string hints, bool isRequired)
            : base(label, hints)
        {
            IsRequired = isRequired;
        }

        public void Validate(IList<ValidationError> errors)
        {
            ValidateCore(errors);
        }

        protected virtual void ValidateCore(IList<ValidationError> errors)
        {
        }
    }
}