// ==========================================================================
//  NamedElementProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Core.Schemas
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