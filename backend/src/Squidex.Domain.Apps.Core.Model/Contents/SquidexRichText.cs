// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Text.RichText.Model;

namespace Squidex.Domain.Apps.Core.Contents;

public static class SquidexRichText
{
    public static class NodeTypes
    {
        public const string ContentLink = "contentLink";
    }

    private class ExtendedOptions : RichTextOptions
    {
        public override bool IsSupportedMarkType(string type)
        {
            return base.IsSupportedMarkType(type) || IsExtension(type);
        }

        public override bool IsSupportedNodeType(string type)
        {
            return base.IsSupportedNodeType(type) || IsExtension(type);
        }

        private static bool IsExtension(string type)
        {
            return type.StartsWith("x-", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static readonly RichTextOptions Options = new ExtendedOptions
    {
        NodeTypes =
        [
            ..RichTextOptions.Default.NodeTypes,
            NodeTypes.ContentLink,
        ],
        MarkTypes = RichTextOptions.Default.MarkTypes,
    };
}
