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
        public bool IsRequired { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            foreach (var error in ValidateCore())
            {
                errors.Add(error);
            }
        }

        protected virtual IEnumerable<ValidationError> ValidateCore()
        {
            yield break;
        }

        public FieldProperties Clone()
        {
            return (FieldProperties) MemberwiseClone();
        }
    }
}