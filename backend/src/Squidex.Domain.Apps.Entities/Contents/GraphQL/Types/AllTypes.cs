// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class AllTypes
    {
        public static readonly Type None = typeof(NoopGraphType);

        public static readonly IGraphType Int = new IntGraphType();

        public static readonly IGraphType DomainId = new StringGraphType();

        public static readonly IGraphType Long = new LongGraphType();

        public static readonly IGraphType Guid = new GuidGraphType();

        public static readonly IGraphType Date = new InstantGraphType();

        public static readonly IGraphType Json = new JsonGraphType();

        public static readonly IGraphType Float = new FloatGraphType();

        public static readonly IGraphType String = new StringGraphType();

        public static readonly IGraphType Strings = new ListGraphType(new NonNullGraphType(new StringGraphType()));

        public static readonly IGraphType Boolean = new BooleanGraphType();

        public static readonly IGraphType AssetType = new EnumerationGraphType<AssetType>();

        public static readonly IGraphType NonNullInt = new NonNullGraphType(Int);

        public static readonly IGraphType NonNullDomainId = new NonNullGraphType(DomainId);

        public static readonly IGraphType NonNullLong = new NonNullGraphType(Long);

        public static readonly IGraphType NonNullGuid = new NonNullGraphType(Guid);

        public static readonly IGraphType NonNullDate = new NonNullGraphType(Date);

        public static readonly IGraphType NonNullFloat = new NonNullGraphType(Float);

        public static readonly IGraphType NonNullString = new NonNullGraphType(String);

        public static readonly IGraphType NonNullStrings = new NonNullGraphType(Strings);

        public static readonly IGraphType NonNullBoolean = new NonNullGraphType(Boolean);

        public static readonly IGraphType NonNullAssetType = new NonNullGraphType(AssetType);

        public static readonly IGraphType NoopJson = new NoopGraphType(Json);
    }
}
