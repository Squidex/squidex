// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.Contracts;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class NestedField : FieldBase, INestedField
    {
        public bool IsLocked { get; private set; }

        public bool IsHidden { get; private set; }

        public bool IsDisabled { get; private set; }

        public abstract FieldProperties RawProperties { get; }

        protected NestedField(long id, string name, IFieldSettings? settings = null)
            : base(id, name)
        {
            if (settings != null)
            {
                IsLocked = settings.IsLocked;
                IsHidden = settings.IsHidden;
                IsDisabled = settings.IsDisabled;
            }
        }

        [Pure]
        public NestedField Lock()
        {
            if (IsLocked)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsLocked = true;
            });
        }

        [Pure]
        public NestedField Hide()
        {
            if (IsHidden)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsHidden = true;
            });
        }

        [Pure]
        public NestedField Show()
        {
            if (!IsHidden)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsHidden = false;
            });
        }

        [Pure]
        public NestedField Disable()
        {
            if (IsDisabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsDisabled = true;
            });
        }

        [Pure]
        public NestedField Enable()
        {
            if (!IsDisabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsDisabled = false;
            });
        }

        public abstract T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, TArgs args);

        public abstract NestedField Update(FieldProperties newProperties);

        protected NestedField Clone(Action<NestedField> updater)
        {
            var clone = (NestedField)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
