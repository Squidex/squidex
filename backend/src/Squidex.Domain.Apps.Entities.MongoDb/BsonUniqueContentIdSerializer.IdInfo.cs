// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

public partial class BsonUniqueContentIdSerializer
{
    private readonly record struct IdInfo(int Length, bool IsGuid, Guid AsGuid, string Source)
    {
        public const byte GuidLength = 16;
        public const byte GuidIndicator = byte.MaxValue;
        public const byte LongIdIndicator = byte.MaxValue - 1;
        public const byte SizeOfInt = 4;
        public const byte SizeOfByte = 1;

        public bool IsEmpty => IsGuid && AsGuid == default;

        public static IdInfo Create(DomainId id)
        {
            var source = id.ToString();

            if (Guid.TryParse(source, out var guid))
            {
                return new IdInfo(GuidLength, true, guid, source);
            }

            return new IdInfo(Encoding.UTF8.GetByteCount(source), false, default, source);
        }

        public int Size(bool writeEmpty)
        {
            if (IsEmpty && !writeEmpty)
            {
                return 0;
            }

            if (Length >= LongIdIndicator)
            {
                return SizeOfByte + SizeOfInt + Length;
            }

            return SizeOfByte + Length;
        }

        public int Write(Span<byte> buffer)
        {
            if (Length >= LongIdIndicator)
            {
                buffer[0] = LongIdIndicator;

                Write(buffer[1..], WriteLengthAsInt);
            }
            else
            {
                Write(buffer, WriteLengthAsByte);
            }

            return Size(false);
        }

        private int Write(Span<byte> buffer, WriteLength writeLength)
        {
            int lengthSize;
            if (IsGuid)
            {
                // Special length indicator for all guids.
                lengthSize = writeLength(buffer, GuidIndicator);

                AsGuid.TryWriteBytes(buffer[lengthSize..]);
            }
            else
            {
                // We assume that we use relatively small IDs, not longer than 253 bytes.
                lengthSize = writeLength(buffer, Length);

                Encoding.UTF8.GetBytes(Source, buffer[lengthSize..]);
            }

            return lengthSize + Length;
        }

        public static (DomainId Id, int Length) Read(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                return default;
            }

            if (buffer[0] == LongIdIndicator)
            {
                var (id, read) = Read(buffer[1..], ReadLengthAsInt);

                return (id, read + 1);
            }
            else
            {
                return Read(buffer, ReadLengthAsByte);
            }
        }

        private static (DomainId Id, int Length) Read(ReadOnlySpan<byte> buffer, ReadLength readLength)
        {
            var (length, offset) = readLength(buffer);

            if (length == GuidIndicator)
            {
                // For guids the size is just an indicator and we use a hardcoded size.
                buffer = buffer.Slice(offset, GuidLength);

                return (DomainId.Create(new Guid(buffer)), offset + GuidLength);
            }
            else
            {
                // For strings the size is correct.
                buffer = buffer.Slice(offset, length);

                return (DomainId.Create(Encoding.UTF8.GetString(buffer)), offset + length);
            }
        }

        private static int WriteLengthAsByte(Span<byte> buffer, int length)
        {
            buffer[0] = (byte)length;

            return SizeOfByte;
        }

        private static int WriteLengthAsInt(Span<byte> buffer, int length)
        {
            BitConverter.TryWriteBytes(buffer, length);

            return SizeOfInt;
        }

        private static (int, int) ReadLengthAsByte(ReadOnlySpan<byte> buffer)
        {
            return (buffer[0], SizeOfByte);
        }

        private static (int, int) ReadLengthAsInt(ReadOnlySpan<byte> buffer)
        {
            return (BitConverter.ToInt32(buffer), SizeOfInt);
        }

        private delegate int WriteLength(Span<byte> buffer, int length);

        private delegate (int, int) ReadLength(ReadOnlySpan<byte> buffer);
    }
}
