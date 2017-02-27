// ==========================================================================
//  JsonFieldModel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Core.Schemas.Json
{
    public sealed class JsonFieldModel
    {
        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public bool IsDisabled { get; set; }

        public FieldProperties Properties { get; set; }
    }
}