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
    public abstract class RootField : Cloneable<RootField>, IRootField
    {
        private readonly long fieldId;
        private readonly string fieldName;
        private readonly Partitioning partitioning;
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

        public Partitioning Partitioning
        {
            get { return partitioning; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected RootField(long id, string name, Partitioning partitioning)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));
            Guard.NotNull(partitioning, nameof(partitioning));

            fieldId = id;
            fieldName = name;

            this.partitioning = partitioning;
        }

        [Pure]
        public RootField Lock()
        {
            return Clone(clone =>
            {
                clone.isLocked = true;
            });
        }

        [Pure]
        public RootField Hide()
        {
            return Clone(clone =>
            {
                clone.isHidden = true;
            });
        }

        [Pure]
        public RootField Show()
        {
            return Clone(clone =>
            {
                clone.isHidden = false;
            });
        }

        [Pure]
        public RootField Disable()
        {
            return Clone(clone =>
            {
                clone.isDisabled = true;
            });
        }

        [Pure]
        public RootField Enable()
        {
            return Clone(clone =>
            {
                clone.isDisabled = false;
            });
        }

        public abstract T Accept<T>(IFieldVisitor<T> visitor);

        public abstract RootField Update(FieldProperties newProperties);
    }
}