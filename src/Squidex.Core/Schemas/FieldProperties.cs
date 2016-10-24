// ==========================================================================
//  FieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    public abstract class FieldProperties : NamedElementProperties, IValidatable
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