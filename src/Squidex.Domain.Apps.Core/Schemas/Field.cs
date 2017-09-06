// ==========================================================================
//  Field.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class Field : CloneableBase
    {
        private readonly Lazy<List<IValidator>> validators;
        private readonly long fieldId;
        private readonly Partitioning partitioning;
        private string fieldName;
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

        public Partitioning Paritioning
        {
            get { return partitioning; }
        }

        public IReadOnlyList<IValidator> Validators
        {
            get { return validators.Value; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected Field(long id, string name, Partitioning partitioning)
        {
            Guard.ValidPropertyName(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));
            Guard.NotNull(partitioning, nameof(partitioning));

            fieldId = id;
            fieldName = name;

            this.partitioning = partitioning;

            validators = new Lazy<List<IValidator>>(() => new List<IValidator>(CreateValidators()));
        }

        protected abstract Field UpdateInternal(FieldProperties newProperties);

        public abstract object ConvertValue(JToken value);

        public Field Lock()
        {
            return Clone<Field>(clone => clone.isLocked = true);
        }

        public Field Hide()
        {
            return Clone<Field>(clone => clone.isHidden = true);
        }

        public Field Show()
        {
            return Clone<Field>(clone => clone.isHidden = false);
        }

        public Field Disable()
        {
            return Clone<Field>(clone => clone.isDisabled = true);
        }

        public Field Enable()
        {
            return Clone<Field>(clone => clone.isDisabled = false);
        }

        public Field Update(FieldProperties newProperties)
        {
            ThrowIfLocked();

            return UpdateInternal(newProperties);
        }

        public Field Rename(string newName)
        {
            ThrowIfLocked();
            ThrowIfSameName(newName);

            return Clone<Field>(clone => clone.fieldName = newName);
        }

        private void ThrowIfLocked()
        {
            if (isLocked)
            {
                throw new DomainException($"Field {fieldId} is locked.");
            }
        }

        private void ThrowIfSameName(string newName)
        {
            if (!newName.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException($"Cannot rename the field '{fieldName}' ({fieldId})", error);
            }
        }

        public void AddToEdmType(EdmStructuredType edmType, PartitionResolver partitionResolver, string schemaName, Func<EdmComplexType, EdmComplexType> typeResolver)
        {
            Guard.NotNull(edmType, nameof(edmType));
            Guard.NotNull(typeResolver, nameof(typeResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var edmValueType = CreateEdmType();

            if (edmValueType == null)
            {
                return;
            }

            var partitionType = typeResolver(new EdmComplexType("Squidex", $"{schemaName}{Name.ToPascalCase()}Property"));
            var partition = partitionResolver(partitioning);

            foreach (var partitionItem in partition)
            {
                partitionType.AddStructuralProperty(partitionItem.Key, edmValueType);
            }

            edmType.AddStructuralProperty(Name.EscapeEdmField(), new EdmComplexTypeReference(partitionType, false));
        }

        public void AddToJsonSchema(JsonSchema4 schema, PartitionResolver partitionResolver, string schemaName, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var partitionProperty = CreateProperty();
            var partitionObject = new JsonSchema4 { Type = JsonObjectType.Object, AllowAdditionalProperties = false };
            var partition = partitionResolver(partitioning);

            foreach (var partitionItem in partition)
            {
                var partitionItemProperty = new JsonProperty { Description = partitionItem.Name, IsRequired = RawProperties.IsRequired };

                PrepareJsonSchema(partitionItemProperty, schemaResolver);

                partitionObject.Properties.Add(partitionItem.Key, partitionItemProperty);
            }

            partitionProperty.SchemaReference = schemaResolver($"{schemaName}{Name.ToPascalCase()}Property", partitionObject);

            schema.Properties.Add(Name, partitionProperty);
        }

        public JsonProperty CreateProperty()
        {
            var jsonProperty = new JsonProperty { IsRequired = RawProperties.IsRequired, Type = JsonObjectType.Object };

            if (!string.IsNullOrWhiteSpace(RawProperties.Hints))
            {
                jsonProperty.Description = RawProperties.Hints;
            }
            else
            {
                jsonProperty.Description = Name;
            }

            if (!string.IsNullOrWhiteSpace(RawProperties.Hints))
            {
                jsonProperty.Description += $" ({RawProperties.Hints}).";
            }

            return jsonProperty;
        }

        protected abstract IEnumerable<IValidator> CreateValidators();

        protected abstract IEdmTypeReference CreateEdmType();

        protected abstract void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver);
    }
}