// ==========================================================================
//  SchemaProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Core.Schemas
{
    public sealed class SchemaProperties : NamedElementProperties
    {
        public SchemaProperties(
            string label, 
            string hints)
            : base(label, hints)
        {
        }
    }
}