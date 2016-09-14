// ==========================================================================
//  SchemaProperties.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Core.Schemas
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