// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Security.Cryptography;

namespace Squidex.Infrastructure.Assets
{
    public sealed class HasherStream : Stream
    {
        private readonly Stream inner;
        private readonly IncrementalHash hasher;

        public override bool CanRead
        {
            get { return inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return inner.Length; }
        }

        public override long Position
        {
            get { return inner.Position; }
            set { throw new NotSupportedException(); }
        }

        public HasherStream(Stream inner, HashAlgorithmName hashAlgorithmName)
        {
            Guard.NotNull(inner, nameof(inner));

            if (!inner.CanRead)
            {
                throw new ArgumentException("Inner stream must be readable.");
            }

            this.inner = inner;

            hasher = IncrementalHash.CreateHash(hashAlgorithmName);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = inner.Read(buffer, offset, count);

            if (read > 0)
            {
                hasher.AppendData(buffer, offset, read);
            }

            return read;
        }

        public byte[] GetHashAndReset()
        {
            return hasher.GetHashAndReset();
        }

        public string GetHashStringAndReset()
        {
            return Convert.ToBase64String(GetHashAndReset());
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
