// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class InputFieldVisitor : IFieldVisitor<IGraphType>
    {
        public static readonly InputFieldVisitor Default = new InputFieldVisitor();

        private InputFieldVisitor()
        {
        }

        public IGraphType Visit(IField<AssetsFieldProperties> field)
        {
            return AllTypes.ListOfNonNullGuid;
        }

        public IGraphType Visit(IField<BooleanFieldProperties> field)
        {
            return AllTypes.Boolean;
        }

        public IGraphType Visit(IField<DateTimeFieldProperties> field)
        {
            return AllTypes.Date;
        }

        public IGraphType Visit(IField<GeolocationFieldProperties> field)
        {
            return AllTypes.GeolocationInput;
        }

        public IGraphType Visit(IField<JsonFieldProperties> field)
        {
            return AllTypes.NoopJson;
        }

        public IGraphType Visit(IField<NumberFieldProperties> field)
        {
            return AllTypes.Float;
        }

        public IGraphType Visit(IField<ReferencesFieldProperties> field)
        {
            return AllTypes.ListOfNonNullGuid;
        }

        public IGraphType Visit(IField<StringFieldProperties> field)
        {
            return AllTypes.String;
        }

        public IGraphType Visit(IField<TagsFieldProperties> field)
        {
            return AllTypes.ListOfNonNullString;
        }
    }
}
