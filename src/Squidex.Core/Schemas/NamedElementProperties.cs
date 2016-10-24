// ==========================================================================
//  NamedElementProperties.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Core.Schemas
{
    public abstract class NamedElementProperties
    {
        private readonly string label;
        private readonly string hints;

        public string Label
        {
            get { return label; }
        }

        public string Hints
        {
            get { return hints; }
        }

        protected NamedElementProperties(string label, string hints)
        {
            this.label = label;
            this.hints = hints;
        }
    }
}