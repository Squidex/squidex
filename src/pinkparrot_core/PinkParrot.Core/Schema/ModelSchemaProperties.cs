// ==========================================================================
//  ModelSchemaMetadata.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Core.Schema
{
    public sealed class ModelSchemaProperties : NamedElementProperties
    {
        public ModelSchemaProperties(
            string name, 
            string label, 
            string hints)
            : base(name, label, hints)
        {
        }
    }
}