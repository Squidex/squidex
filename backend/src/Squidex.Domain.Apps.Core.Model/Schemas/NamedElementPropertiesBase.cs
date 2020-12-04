// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Linq;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class NamedElementPropertiesBase : Freezable
    {
        public string? Label
        {
            get
            {
                if (LocalizedLabel == null)
                {
                    return string.Empty;
                }

                return LocalizedLabel.GetLocalizedValue();
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    LocalizedLabel = new LocalizedValue<string>
                    {
                        ["en"] = value
                    };
                }
                else
                {
                    LocalizedLabel = null;
                }
            }
        }

        public string? Hints
        {
            get
            {
                if (LocalizedHints == null)
                {
                    return string.Empty;
                }

                return LocalizedHints.GetLocalizedValue();
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    LocalizedHints = new LocalizedValue<string>
                    {
                        ["en"] = value
                    };
                }
                else
                {
                    LocalizedHints = null;
                }
            }
        }

        public LocalizedValue<string>? LocalizedLabel { get; set; }

        public LocalizedValue<string>? LocalizedHints { get; set; }
    }
}