// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class NamedElementPropertiesBase : Freezable
    {
        private string label;
        private string hints;

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
    }
}