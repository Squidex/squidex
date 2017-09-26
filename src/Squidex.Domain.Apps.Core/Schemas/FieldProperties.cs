﻿// ==========================================================================
//  FieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldProperties : NamedElementPropertiesBase, IValidatable
    {
        private bool isRequired;
        private bool isListField;
        private string placeholder;

        public bool IsRequired
        {
            get
            {
                return isRequired;
            }
            set
            {
                ThrowIfFrozen();

                isRequired = value;
            }
        }

        public bool IsListField
        {
            get
            {
                return isListField;
            }
            set
            {
                ThrowIfFrozen();

                isListField = value;
            }
        }

        public string Placeholder
        {
            get
            {
                return placeholder;
            }
            set
            {
                ThrowIfFrozen();

                placeholder = value;
            }
        }

        public abstract JToken GetDefaultValue();

        public virtual bool ShouldApplyDefaultValue(JToken value)
        {
            return value.IsNull();
        }

        public void Validate(IList<ValidationError> errors)
        {
            foreach (var error in ValidateCore())
            {
                errors.Add(error);
            }
        }

        protected abstract IEnumerable<ValidationError> ValidateCore();
    }
}