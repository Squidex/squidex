﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Validation
{
    public sealed class DependencyValidatorsFactory : IValidatorsFactory
    {
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;

        public DependencyValidatorsFactory(IAssetRepository assetRepository, IContentRepository contentRepository)
        {
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        public IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, FieldValidatorFactory createFieldValidator)
        {
            if (field is IField<AssetsFieldProperties> assetsField)
            {
                var checkAssets = new CheckAssets(async ids =>
                {
                    return await assetRepository.QueryAsync(context.AppId.Id, new HashSet<DomainId>(ids));
                });

                yield return new AssetsValidator(assetsField.Properties, checkAssets);
            }

            if (field is IField<ReferencesFieldProperties> referencesField)
            {
                var checkReferences = new CheckContentsByIds(async ids =>
                {
                    return await contentRepository.QueryIdsAsync(context.AppId.Id, ids, SearchScope.All);
                });

                yield return new ReferencesValidator(referencesField.Properties.SchemaIds, checkReferences);
            }

            if (field is IField<NumberFieldProperties> numberField && numberField.Properties.IsUnique)
            {
                var checkUniqueness = new CheckUniqueness(async filter =>
                {
                    return await contentRepository.QueryIdsAsync(context.AppId.Id, context.SchemaId.Id, filter);
                });

                yield return new UniqueValidator(checkUniqueness);
            }

            if (field is IField<StringFieldProperties> stringField && stringField.Properties.IsUnique)
            {
                var checkUniqueness = new CheckUniqueness(async filter =>
                {
                    return await contentRepository.QueryIdsAsync(context.AppId.Id, context.SchemaId.Id, filter);
                });

                yield return new UniqueValidator(checkUniqueness);
            }
        }
    }
}
