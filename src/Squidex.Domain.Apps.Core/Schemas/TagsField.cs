// ==========================================================================
//  TagsField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class TagsField : Field<TagsFieldProperties>
    {
        private static readonly ImmutableList<string> EmptyTags = ImmutableList<string>.Empty;

        public TagsField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new TagsFieldProperties())
        {
        }

        public TagsField(long id, string name, Partitioning partitioning, TagsFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired || Properties.MinItems.HasValue || Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator<string>(Properties.IsRequired, Properties.MinItems, Properties.MaxItems);
            }

            yield return new CollectionItemValidator<string>(new RequiredStringValidator());
        }

        public override object ConvertValue(JToken value)
        {
            return value.ToObject<List<string>>();
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
