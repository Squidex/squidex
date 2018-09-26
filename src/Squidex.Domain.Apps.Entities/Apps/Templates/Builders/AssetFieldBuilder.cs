// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Templates.Builders
{
    public class AssetFieldBuilder : FieldBuilder
    {
        public AssetFieldBuilder(CreateSchemaField field)
            : base(field)
        {
        }

        public AssetFieldBuilder MustBeImage()
        {
            Properties<AssetsFieldProperties>().MustBeImage = true;

            return this;
        }

        public AssetFieldBuilder RequireSingle()
        {
            Properties<AssetsFieldProperties>().MaxItems = 2;

            return this;
        }
    }
}
