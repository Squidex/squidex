// ==========================================================================
//  NamedElementPropertiesBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class NamedElementPropertiesBase
    {
        public string Label { get; set; }

        public string Hints { get; set; }
    }
}