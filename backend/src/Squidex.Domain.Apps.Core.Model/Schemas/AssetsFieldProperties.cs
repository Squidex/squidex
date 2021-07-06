﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record AssetsFieldProperties : FieldProperties
    {
        public AssetPreviewMode PreviewMode { get; init; }

        public LocalizedValue<ImmutableList<string>?> DefaultValues { get; init; }

        public ImmutableList<string>? DefaultValue { get; init; }

        public string? FolderId { get; init; }

        public int? MinItems { get; init; }

        public int? MaxItems { get; init; }

        public int? MinWidth { get; init; }

        public int? MaxWidth { get; init; }

        public int? MinHeight { get; init; }

        public int? MaxHeight { get; init; }

        public int? MinSize { get; init; }

        public int? MaxSize { get; init; }

        public int? AspectWidth { get; init; }

        public int? AspectHeight { get; init; }

        public bool MustBeImage { get; init; }

        public bool AllowDuplicates { get; init; }

        public bool ResolveFirst { get; init; }

        public bool ResolveImage
        {
            init => ResolveFirst = value;
        }

        public ImmutableList<string>? AllowedExtensions { get; set; }

        public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
        {
            return visitor.Visit((IField<AssetsFieldProperties>)field, args);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.Assets(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            return Fields.Assets(id, name, this, settings);
        }
    }
}
