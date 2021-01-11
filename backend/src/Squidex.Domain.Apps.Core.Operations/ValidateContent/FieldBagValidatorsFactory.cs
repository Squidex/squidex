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
    internal sealed class FieldBagValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>, None>
    {
        private static readonly FieldBagValidatorsFactory Instance = new FieldBagValidatorsFactory();

        private FieldBagValidatorsFactory()
        {
        }

        public static IEnumerable<IValidator> CreateValidators(IField field)
        {
            return field.Accept(Instance, None.Value);
        }

        public IEnumerable<IValidator> Visit(IArrayField field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<BooleanFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<DateTimeFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<GeolocationFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<JsonFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<NumberFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<ReferencesFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<StringFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<TagsFieldProperties> field, None args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<UIFieldProperties> field, None args)
        {
            yield return NoValueValidator.Instance;
        }
    }
}
