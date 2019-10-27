// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class AllTypes
    {
        public static readonly Type None = typeof(NoopGraphType);

        public static readonly Type NonNullTagsType = typeof(NonNullGraphType<ListGraphType<NonNullGraphType<StringGraphType>>>);

        public static readonly IGraphType Int = new IntGraphType();

        public static readonly IGraphType Guid = new GuidGraphType2();

        public static readonly IGraphType Date = new InstantGraphType();

        public static readonly IGraphType Json = new JsonGraphType();

        public static readonly IGraphType Float = new FloatGraphType();

        public static readonly IGraphType String = new StringGraphType();

        public static readonly IGraphType Boolean = new BooleanGraphType();

        public static readonly IGraphType NonNullInt = new NonNullGraphType(Int);

        public static readonly IGraphType NonNullGuid = new NonNullGraphType(Guid);

        public static readonly IGraphType NonNullDate = new NonNullGraphType(Date);

        public static readonly IGraphType NonNullFloat = new NonNullGraphType(Float);

        public static readonly IGraphType NonNullString = new NonNullGraphType(String);

        public static readonly IGraphType NonNullBoolean = new NonNullGraphType(Boolean);

        public static readonly IGraphType NoopDate = new NoopGraphType(Date);

        public static readonly IGraphType NoopJson = new NoopGraphType(Json);

        public static readonly IGraphType NoopFloat = new NoopGraphType(Float);

        public static readonly IGraphType NoopString = new NoopGraphType(String);

        public static readonly IGraphType NoopBoolean = new NoopGraphType(Boolean);

        public static readonly IGraphType NoopTags = new NoopGraphType("Tags");

        public static readonly IGraphType NoopGeolocation = new NoopGraphType("Geolocation");
    }
}
