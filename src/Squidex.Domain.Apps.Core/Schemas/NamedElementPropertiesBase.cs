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
        private string label;
        private string hints;

        protected bool IsFrozen { get; private set; }

        public string Label
        {
            get
            {
                return label;
            }
            set
            {
                ThrowIfFrozen();

                label = value;
            }
        }

        public string Hints
        {
            get
            {
                return hints;
            }
            set
            {
                ThrowIfFrozen();

                hints = value;
            }
        }

        protected void ThrowIfFrozen()
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException("Object is frozen.");
            }
        }

        public void Freeze()
        {
            IsFrozen = true;
        }
    }
}