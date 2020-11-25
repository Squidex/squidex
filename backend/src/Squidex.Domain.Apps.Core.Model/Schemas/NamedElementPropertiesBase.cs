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
                if (DefaultValuesLabel == null)
                {
                    return string.Empty;
                }

                if (DefaultValuesLabel.TryGetValue(CultureInfo.CurrentUICulture.ToString(), out var current))
                {
                    return current;
                }

                if (DefaultValuesLabel.TryGetValue("en", out var english))
                {
                    return english;
                }

                return DefaultValuesLabel.Values.FirstOrDefault();
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    DefaultValuesLabel = new LocalizedValue<string>
                    {
                        ["en"] = value
                    };
                }
                else
                {
                    DefaultValuesLabel = null;
                }
            }
        }

        public string? Hints
        {
            get
            {
                if (DefaultValuesHints == null)
                {
                    return string.Empty;
                }

                if (DefaultValuesHints.TryGetValue(CultureInfo.CurrentUICulture.ToString(), out var current))
                {
                    return current;
                }

                if (DefaultValuesHints.TryGetValue("en", out var english))
                {
                    return english;
                }

                return DefaultValuesHints.Values.FirstOrDefault();
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    DefaultValuesHints = new LocalizedValue<string>
                    {
                        ["en"] = value
                    };
                }
                else
                {
                    DefaultValuesHints = null;
                }
            }
        }

        public LocalizedValue<string>? DefaultValuesLabel { get; set; }

        public LocalizedValue<string>? DefaultValuesHints { get; set; }
    }
}