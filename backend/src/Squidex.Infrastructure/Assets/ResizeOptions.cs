// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Infrastructure.Assets
{
    public sealed class ResizeOptions
    {
        public ImageFormat Format { get; set; }

        public ResizeMode Mode { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Quality { get; set; }

        public float? FocusX { get; set; }

        public float? FocusY { get; set; }

        public bool KeepFormat { get; set; }

        public bool IsValid
        {
            get { return Width > 0 || Height > 0 || Quality > 0 || Format != ImageFormat.Auto; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Width);
            sb.Append("_");
            sb.Append(Height);
            sb.Append("_");
            sb.Append(Mode);

            if (Quality.HasValue)
            {
                sb.Append("_");
                sb.Append(Quality);
            }

            if (FocusX.HasValue)
            {
                sb.Append("_focusX_");
                sb.Append(FocusX);
            }

            if (FocusY.HasValue)
            {
                sb.Append("_focusY_");
                sb.Append(FocusY);
            }

            if (Format != ImageFormat.Auto)
            {
                sb.Append("_format_");
                sb.Append(Format.ToString());
            }

            return sb.ToString();
        }
    }
}
