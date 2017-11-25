// ==========================================================================
//  InconsistentStateException.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Squidex.Infrastructure.States
{
    [Serializable]
    public class InconsistentStateException : Exception
    {
        private readonly string currentEtag;
        private readonly string expectedEtag;

        public string CurrentEtag
        {
            get { return currentEtag; }
        }

        public string ExpectedEtag
        {
            get { return expectedEtag; }
        }

        public InconsistentStateException(string currentEtag, string expectedEtag, Exception ex)
            : base(FormatMessage(currentEtag, expectedEtag), ex)
        {
            this.currentEtag = currentEtag;

            this.expectedEtag = expectedEtag;
        }

        protected InconsistentStateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string currentEtag, string expectedEtag)
        {
            return $"Requested etag {expectedEtag}, but found {currentEtag}.";
        }
    }
}
