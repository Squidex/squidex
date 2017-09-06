// ==========================================================================
//  ReferencesField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class ReferencesField : Field<ReferencesFieldProperties>, IReferenceField
    {
        private static readonly Guid[] EmptyIds = new Guid[0];

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
            if (Properties.SchemaId != Guid.Empty)
            {
                yield return new ReferencesValidator(Properties.IsRequired, Properties.SchemaId, Properties.MinItems, Properties.MaxItems);
            }
        }

        public IEnumerable<Guid> GetReferencedIds(JToken value)
        {
            Guid[] referenceIds;
            try
            {
                referenceIds = value?.ToObject<Guid[]>() ?? EmptyIds;
            }
            catch
            {
                referenceIds = EmptyIds;
            }

            return referenceIds.Union(new[] { Properties.SchemaId });
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
            return new ReferencesValue(value.ToObject<Guid[]>());
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            var itemSchema = schemaResolver("ReferenceItem", new JsonSchema4 { Type = JsonObjectType.String });

            jsonProperty.Type = JsonObjectType.Array;
            jsonProperty.Item = itemSchema;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, !Properties.IsRequired);
        }
    }
}
