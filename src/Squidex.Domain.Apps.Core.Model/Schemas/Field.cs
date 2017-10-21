// ==========================================================================
//  Field.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class Field
    {
        private readonly long fieldId;
        private readonly Partitioning partitioning;
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

        public Partitioning Partitioning
        {
            get { return partitioning; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected Field(long id, string name, Partitioning partitioning)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(partitioning, nameof(partitioning));
            Guard.GreaterThan(id, 0, nameof(id));

            fieldId = id;
            fieldName = name;

            this.partitioning = partitioning;
        }

        public void Lock()
        {
            isLocked = true;
        }

        public void Hide()
        {
            isHidden = true;
        }

        public void Show()
        {
            isHidden = false;
        }

        public void Disable()
        {
            isDisabled = true;
        }

        public void Enable()
        {
            isDisabled = false;
        }

        public abstract void Update(FieldProperties newProperties);

        public abstract T Accept<T>(IFieldVisitor<T> visitor);
    }
}