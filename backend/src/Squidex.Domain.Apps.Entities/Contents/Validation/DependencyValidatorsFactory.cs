// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;

namespace Squidex.Domain.Apps.Entities.Contents.Validation;

public sealed class DependencyValidatorsFactory : IValidatorsFactory
{
    private readonly IAssetRepository assetRepository;
    private readonly IContentRepository contentRepository;

    public DependencyValidatorsFactory(IAssetRepository assetRepository, IContentRepository contentRepository)
    {
        this.assetRepository = assetRepository;
        this.contentRepository = contentRepository;
    }

    public IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, ValidatorFactory createFieldValidator)
    {
        if (context.Mode == ValidationMode.Optimized)
        {
            yield break;
        }

        var isRequired = IsRequired(context, field.RawProperties);

        if (field is IField<AssetsFieldProperties> assetsField)
        {
            var checkAssets = new CheckAssets(async ids =>
            {
                return await assetRepository.QueryAsync(context.Root.AppId.Id, null, Q.Empty.WithIds(ids), default);
            });

            yield return new AssetsValidator(isRequired, assetsField.Properties, checkAssets);
        }

        if (field is IField<ReferencesFieldProperties> referencesField)
        {
            var checkReferences = new CheckContentsByIds(async ids =>
            {
                return await contentRepository.QueryIdsAsync(context.Root.AppId.Id, ids, SearchScope.All, default);
            });

            yield return new ReferencesValidator(isRequired, referencesField.Properties, checkReferences);
        }

        if (field is IField<NumberFieldProperties> numberField && numberField.Properties.IsUnique)
        {
            var checkUniqueness = new CheckUniqueness(async filter =>
            {
                return await contentRepository.QueryIdsAsync(context.Root.AppId.Id, context.Root.SchemaId.Id, filter, default);
            });

            yield return new UniqueValidator(checkUniqueness);
        }

        if (field is IField<StringFieldProperties> stringField && stringField.Properties.IsUnique)
        {
            var checkUniqueness = new CheckUniqueness(async filter =>
            {
                return await contentRepository.QueryIdsAsync(context.Root.AppId.Id, context.Root.SchemaId.Id, filter, default);
            });

            yield return new UniqueValidator(checkUniqueness);
        }
    }

    private static bool IsRequired(ValidationContext context, FieldProperties properties)
    {
        var isRequired = properties.IsRequired;

        if (context.Action == ValidationAction.Publish)
        {
            isRequired = isRequired || properties.IsRequiredOnPublish;
        }

        return isRequired;
    }
}
