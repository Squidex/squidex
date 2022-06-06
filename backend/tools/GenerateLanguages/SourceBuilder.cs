// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace GenerateLanguages
{
    internal sealed class SourceBuilder
    {
        private readonly StringBuilder builder = new StringBuilder();
        private int intentation;

        public SourceBuilder StartBlock()
        {
            intentation += 4;

            return this;
        }

        public SourceBuilder WriteLine(string line)
        {
            if (line.Equals("}", StringComparison.Ordinal))
            {
                EndBlock();
            }

            for (var i = 0; i < intentation; i++)
            {
                builder.Append(' ');
            }

            builder.AppendLine(line);

            if (line.Equals("{", StringComparison.Ordinal))
            {
                StartBlock();
            }

            return this;
        }

        public SourceBuilder WriteLine()
        {
            builder.AppendLine();

            return this;
        }

        public SourceBuilder EndBlock()
        {
            intentation -= 4;

            return this;
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
