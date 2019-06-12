// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class FieldBagValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>>
    {
        private static readonly FieldBagValidatorsFactory Instance = new FieldBagValidatorsFactory();

        private FieldBagValidatorsFactory()
        {
        }

        public static IEnumerable<IValidator> CreateValidators(IField field)
        {
            Guard.NotNull(field, nameof(field));

            return field.Accept(Instance);
        }

        public IEnumerable<IValidator> Visit(IArrayField field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<BooleanFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<DateTimeFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<GeolocationFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<JsonFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<NumberFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<ReferencesFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<StringFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<TagsFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<UIFieldProperties> field)
        {
            yield return NoValueValidator.Instance;
        }
    }
}
