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
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Core.Schemas
{
    public abstract class Field
    {
        private readonly Lazy<List<IValidator>> validators;
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

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        protected Field(long id, string name)
        {
            Guard.ValidSlug(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));

            this.id = id;

            this.name = name;

            validators = new Lazy<List<IValidator>>(() => new List<IValidator>(CreateValidators()));
        }

        public abstract Field Update(FieldProperties newProperties);

        public async Task ValidateAsync(PropertyValue property, ICollection<string> errors)
        {
            Guard.NotNull(property, nameof(property));

            var value = ConvertValue(property);

            var tempErrors = new List<string>();

            foreach (var validator in validators.Value)
            {
                await validator.ValidateAsync(value, tempErrors);
            }

            foreach (var error in tempErrors)
            {
                errors.Add(error.Replace("<FIELD>", name));
            }
        }

        protected abstract IEnumerable<IValidator> CreateValidators();

        protected abstract object ConvertValue(PropertyValue property);

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

        public abstract Field Clone();

        public abstract FieldProperties CloneProperties();
    }
}