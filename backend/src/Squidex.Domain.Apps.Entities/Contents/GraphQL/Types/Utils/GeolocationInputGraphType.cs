// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils
{
    public sealed class GeolocationInputGraphType : InputObjectGraphType
    {
        public static readonly IGraphType Nullable = new GeolocationInputGraphType();

        public static readonly IGraphType NonNull = new NonNullGraphType(Nullable);

        private GeolocationInputGraphType()
        {
            Name = "GeolocationInputDto";

            AddField(new FieldType
            {
                Name = "latitude",
                ResolvedType = AllTypes.NonNullFloat
            });

            AddField(new FieldType
            {
                Name = "longitude",
                ResolvedType = AllTypes.NonNullFloat
            });
        }
    }
}
