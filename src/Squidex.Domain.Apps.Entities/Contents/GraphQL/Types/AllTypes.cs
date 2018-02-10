// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class AllTypes
    {
        public static readonly Type None = typeof(NoopGraphType);

        public static readonly IGraphType Int = new IntGraphType();

        public static readonly IGraphType Guid = new GuidGraphType();

        public static readonly IGraphType Date = new DateGraphType();

        public static readonly IGraphType Float = new FloatGraphType();

        public static readonly IGraphType String = new StringGraphType();

        public static readonly IGraphType Boolean = new BooleanGraphType();

        public static readonly IGraphType NonNullInt = new NonNullGraphType(new IntGraphType());

        public static readonly IGraphType NonNullGuid = new NonNullGraphType(new GuidGraphType());

        public static readonly IGraphType NonNullDate = new NonNullGraphType(new DateGraphType());

        public static readonly IGraphType NonNullFloat = new NonNullGraphType(new FloatGraphType());

        public static readonly IGraphType NonNullString = new NonNullGraphType(new StringGraphType());

        public static readonly IGraphType NonNullBoolean = new NonNullGraphType(new BooleanGraphType());

        public static readonly IGraphType ListOfNonNullGuid = new ListGraphType(new NonNullGraphType(new GuidGraphType()));

        public static readonly IGraphType ListOfNonNullString = new ListGraphType(new NonNullGraphType(new StringGraphType()));

        public static readonly IGraphType NoopInt = new NoopGraphType("Int");

        public static readonly IGraphType NoopGuid = new NoopGraphType("Guid");

        public static readonly IGraphType NoopDate = new NoopGraphType("Date");

        public static readonly IGraphType NoopJson = new NoopGraphType("Json");

        public static readonly IGraphType NoopTags = new NoopGraphType("Tags");

        public static readonly IGraphType NoopFloat = new NoopGraphType("Float");

        public static readonly IGraphType NoopString = new NoopGraphType("String");

        public static readonly IGraphType NoopBoolean = new NoopGraphType("Boolean");

        public static readonly IGraphType NoopGeolocation = new NoopGraphType("Geolocation");

        public static readonly IGraphType CommandVersion = new CommandVersionGraphType();

        public static readonly IGraphType GeolocationInput = new GeolocationInputGraphType();
    }
}
