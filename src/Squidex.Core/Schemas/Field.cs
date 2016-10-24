// ==========================================================================
//  Field.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Core.Schemas
{
    public abstract class Field
    {
        private readonly long id;
        private string name;
        private bool isDisabled;
        private bool isHidden;

        public long Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Hints
        {
            get { return RawProperties.Hints; }
        }

        public string Label
        {
            get { return RawProperties.Label; }
        }

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        public bool IsRequired
        {
            get { return RawProperties.IsRequired; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected Field(long id, string name)
        {
            Guard.GreaterThan(id, 0, nameof(id));
            Guard.ValidSlug(name, nameof(name));

            this.id = id;

            this.name = name;
        }

        public abstract Field Update(FieldProperties newProperties);

        public Task ValidateAsync(PropertyValue property, ICollection<string> errors)
        {
            Guard.NotNull(property, nameof(property));
            
            if (IsRequired && property.RawValue == null)
            {
                errors.Add("Field is required");
            }

            if (property.RawValue == null)
            {
                return TaskHelper.Done;
            }

            return ValidateCoreAsync(property, errors);
        }

        protected virtual Task ValidateCoreAsync(PropertyValue property, ICollection<string> errors)
        {
            return TaskHelper.Done;
        }

        public Field Hide()
        {
            return Update<Field>(clone => clone.isHidden = true);
        }

        public Field Show()
        {
            return Update<Field>(clone => clone.isHidden = false);
        }

        public Field Disable()
        {
            return Update<Field>(clone => clone.isDisabled = true);
        }

        public Field Enable()
        {
            return Update<Field>(clone => clone.isDisabled = false);
        }

        public Field Rename(string newName)
        {
            if (!newName.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException($"Cannot rename the field '{name}' ({id})", error);
            }

            return Update<Field>(clone => clone.name = newName);
        }

        protected T Update<T>(Action<T> updater) where T : Field
        {
            var clone = (T)Clone();

            updater(clone);

            return clone;
        }

        protected abstract Field Clone();
    }
}