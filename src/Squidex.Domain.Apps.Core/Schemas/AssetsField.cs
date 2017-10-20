// ==========================================================================
//  AssetsField.cs
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
    public sealed class AssetsField : Field<AssetsFieldProperties>, IReferenceField
    {
        private static readonly ImmutableList<Guid> EmptyIds = ImmutableList<Guid>.Empty;

        public AssetsField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new AssetsFieldProperties())
        {
        }

        public AssetsField(long id, string name, Partitioning partitioning, AssetsFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired || Properties.MinItems.HasValue || Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator<Guid>(Properties.IsRequired, Properties.MinItems, Properties.MaxItems);
            }

            yield return new AssetsValidator();
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

            return result ?? EmptyIds;
        }

        public JToken RemoveDeletedReferences(JToken value, ISet<Guid> deletedReferencedIds)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            var oldAssetIds = GetReferencedIds(value).ToArray();
            var newAssetIds = oldAssetIds.Where(x => !deletedReferencedIds.Contains(x)).ToList();

            return newAssetIds.Count != oldAssetIds.Length ? JToken.FromObject(newAssetIds) : value;
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
