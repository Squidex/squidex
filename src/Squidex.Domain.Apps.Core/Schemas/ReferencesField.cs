// ==========================================================================
//  ReferencesField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class ReferencesField : Field<ReferencesFieldProperties>, IReferenceField
    {
        private static readonly ImmutableList<Guid> EmptyIds = ImmutableList<Guid>.Empty;

        public ReferencesField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new ReferencesFieldProperties())
        {
        }

        public ReferencesField(long id, string name, Partitioning partitioning, ReferencesFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired || Properties.MinItems.HasValue || Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator<Guid>(Properties.IsRequired, Properties.MinItems, Properties.MaxItems);
            }

            if (Properties.SchemaId != Guid.Empty)
            {
                yield return new ReferencesValidator(Properties.SchemaId);
            }
        }

        public IEnumerable<Guid> GetReferencedIds(JToken value)
        {
            IEnumerable<Guid> result = null;
            try
            {
                result = value?.ToObject<List<Guid>>();
            }
            catch
            {
                result = EmptyIds;
            }

            return (result ?? EmptyIds).Union(new[] { Properties.SchemaId });
        }

        public JToken RemoveDeletedReferences(JToken value, ISet<Guid> deletedReferencedIds)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            if (deletedReferencedIds.Contains(Properties.SchemaId))
            {
                return new JArray();
            }

            var oldReferenceIds = GetReferencedIds(value).TakeWhile(x => x != Properties.SchemaId).ToArray();
            var newReferenceIds = oldReferenceIds.Where(x => !deletedReferencedIds.Contains(x)).ToList();

            return newReferenceIds.Count != oldReferenceIds.Length ? JToken.FromObject(newReferenceIds) : value;
        }

        public override object ConvertValue(JToken value)
        {
            return value.ToObject<List<Guid>>();
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
