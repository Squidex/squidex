// ==========================================================================
//  BooleanFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("BooleanField")]
    public sealed class BooleanFieldProperties : FieldProperties
    {
        private BooleanFieldEditor editor;
        private bool? defaultValue;

        public bool? DefaultValue
        {
            get { return defaultValue; }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public BooleanFieldEditor Editor
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
        }
    }
}
