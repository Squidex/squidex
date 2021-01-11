// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class NestedField : Cloneable<NestedField>, INestedField
    {
        private readonly long fieldId;
        private readonly string fieldName;
        private bool isDisabled;
        private bool isHidden;
        private bool isLocked;

        public long Id
        {
            get { return fieldId; }
        }

        public string Name
        {
            get { return fieldName; }
        }

        public bool IsLocked
        {
            get { return isLocked; }
        }

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected NestedField(long id, string name, IFieldSettings? settings = null)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));

            fieldId = id;
            fieldName = name;

            if (settings != null)
            {
                isLocked = settings.IsLocked;
                isHidden = settings.IsHidden;
                isDisabled = settings.IsDisabled;
            }
        }

        [Pure]
        public NestedField Lock()
        {
            if (isLocked)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isLocked = true;
            });
        }

        [Pure]
        public NestedField Hide()
        {
            if (isHidden)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isHidden = true;
            });
        }

        [Pure]
        public NestedField Show()
        {
            if (!isHidden)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isHidden = false;
            });
        }

        [Pure]
        public NestedField Disable()
        {
            if (isDisabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isDisabled = true;
            });
        }

        [Pure]
        public NestedField Enable()
        {
            if (!isDisabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isDisabled = false;
            });
        }

        public abstract T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, TArgs args);

        public abstract NestedField Update(FieldProperties newProperties);
    }
}