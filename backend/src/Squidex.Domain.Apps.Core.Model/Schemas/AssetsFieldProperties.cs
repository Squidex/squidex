// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class AssetsFieldProperties : FieldProperties
    {
        public AssetPreviewMode PreviewMode { get; set; }

        public LocalizedValue<string[]?> DefaultValues { get; set; }

        public string[]? DefaultValue { get; set; }

        public int? MinItems { get; set; }

        public int? MaxItems { get; set; }

        public int? MinWidth { get; set; }

        public int? MaxWidth { get; set; }

        public int? MinHeight { get; set; }

        public int? MaxHeight { get; set; }

        public int? MinSize { get; set; }

        public int? MaxSize { get; set; }

        public int? AspectWidth { get; set; }

        public int? AspectHeight { get; set; }

        public bool MustBeImage { get; set; }

        public bool AllowDuplicates { get; set; }

        public bool ResolveFirst { get; set; }

        public bool ResolveImage
        {
            get => ResolveFirst;
            set => ResolveFirst = value;
        }

        public ReadOnlyCollection<string>? AllowedExtensions { get; set; }

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
