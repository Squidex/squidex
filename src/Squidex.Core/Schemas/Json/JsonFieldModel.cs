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
        public string Name;

        public bool IsHidden;

        public bool IsDisabled;

        public FieldProperties Properties;
    }
}