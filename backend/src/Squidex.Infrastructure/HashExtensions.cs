// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace Squidex.Infrastructure;

public static class HashExtensions
{
    private const int MaxStackSize = 128;
    private delegate int HashMethod(ReadOnlySpan<byte> source, Span<byte> destination);
    private delegate string EncodeMethod(ReadOnlySpan<byte> source);
    private static readonly HashMethod HashMD5 = MD5.HashData;
    private static readonly HashMethod HashSHA256 = SHA256.HashData;
    private static readonly HashMethod HashSHA512 = SHA512.HashData;
    private static readonly EncodeMethod EncodeUTF8 = Encoding.UTF8.GetString;
    private static readonly EncodeMethod EncodeBase64 = input => Convert.ToBase64String(input, default);

    public static string ToSha256Base64(this string value)
    {
        return ToHashed(value, HashSHA256, SHA256.HashSizeInBits, Encoding.UTF8, EncodeBase64);
    }

    public static string ToSha256Base64(this ReadOnlySpan<byte> value)
    {
        return ToHashed(value, HashSHA256, SHA256.HashSizeInBits, EncodeBase64);
    }

    public static string ToSha512(this string value)
    {
        return ToHashed(value, HashSHA512, SHA512.HashSizeInBits, Encoding.UTF8, EncodeUTF8);
    }

    public static string ToSha512(this ReadOnlySpan<byte> value)
    {
        return ToHashed(value, HashSHA512, SHA512.HashSizeInBits, EncodeUTF8);
    }

    public static string ToSha256(this string value)
    {
        return ToHashed(value, HashSHA256, SHA256.HashSizeInBits, Encoding.UTF8, EncodeUTF8);
    }

    public static string ToSha256(this ReadOnlySpan<byte> value)
    {
        return ToHashed(value, HashSHA256, SHA256.HashSizeInBits, EncodeUTF8);
    }

    public static string ToMD5(this string value)
    {
        return ToHashed(value, HashMD5, MD5.HashSizeInBits, Encoding.UTF8, EncodeUTF8);
    }

    public static string ToMD5(this ReadOnlySpan<byte> value)
    {
        return ToHashed(value, HashMD5, MD5.HashSizeInBits, EncodeUTF8);
    }

    private static string ToHashed(this string value, HashMethod algorithm, int hashSize, Encoding encoding, EncodeMethod encode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string result;

        var length = encoding.GetByteCount(value);

        if (length > MaxStackSize)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                result = ConvertCore(algorithm, hashSize, value.AsSpan(), buffer, encode, encoding);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        else
        {
            Span<byte> buffer = stackalloc byte[length];

            result = ConvertCore(algorithm, hashSize, value.AsSpan(), buffer, encode, encoding);
        }

        static string ConvertCore(HashMethod algorithm, int hashSize, ReadOnlySpan<char> source, Span<byte> destination, EncodeMethod encode, Encoding encoding)
        {
            var written = encoding.GetBytes(source, destination);

            return ToHashed(destination[..written], algorithm, hashSize, encode);
        }

        return result;
    }

    private static string ToHashed(ReadOnlySpan<byte> bytes, HashMethod algorithm, int hashSize, EncodeMethod encode)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        string result;

        var length = hashSize / 8;

        if (length > MaxStackSize)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                result = ConvertCore(algorithm, bytes, buffer, encode);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        else
        {
            Span<byte> buffer = stackalloc byte[length];

            return ConvertCore(algorithm, bytes, buffer, encode);
        }

        return result;

        static string ConvertCore(HashMethod algorithm, ReadOnlySpan<byte> source, Span<byte> destination, EncodeMethod encode)
        {
            var written = algorithm(source, destination);

            return encode(destination[..written]);
        }
    }

    public static void AppendString(this IncrementalHash hash, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var length = Encoding.Default.GetByteCount(value);

        if (length > MaxStackSize)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                ConvertCore(hash, value, buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        else
        {
            Span<byte> buffer = stackalloc byte[length];

            ConvertCore(hash, value, buffer);
        }

        static void ConvertCore(IncrementalHash hash, string source, Span<byte> destination)
        {
            var written = Encoding.Default.GetBytes(source, destination);

            hash.AppendData(destination[..written]);
        }
    }

    public static void AppendLong(this IncrementalHash hash, long version)
    {
        if (version == 0)
        {
            return;
        }

        Span<byte> buffer = stackalloc byte[sizeof(long)];

        if (BitConverter.TryWriteBytes(buffer, version))
        {
            hash.AppendData(buffer);
        }
    }

    public static string GetHexStringAndReset(this IncrementalHash hash)
    {
        Span<byte> buffer = stackalloc byte[hash.HashLengthInBytes];

        hash.GetHashAndReset(buffer);

        return ToHexString(buffer);
    }

    public static string GetQuotedHexStringAndReset(this IncrementalHash hash)
    {
        Span<byte> buffer = stackalloc byte[hash.HashLengthInBytes];

        hash.GetHashAndReset(buffer);

        return ToQuotedHexString(buffer);
    }

    public static string ToHexString(this ReadOnlySpan<byte> data)
    {
        string result;

        var length = data.Length * 2;

        if (length > MaxStackSize)
        {
            var buffer = ArrayPool<char>.Shared.Rent(length);
            try
            {
                result = ConvertCore(data, buffer, 0);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
        else
        {
            Span<char> buffer = stackalloc char[length];

            result = ConvertCore(data, buffer, 0);
        }

        return result;
    }

    public static string ToQuotedHexString(this ReadOnlySpan<byte> data)
    {
        string result;

        var length = (data.Length * 2) + 2;

        if (length > MaxStackSize)
        {
            var buffer = ArrayPool<char>.Shared.Rent(length);
            try
            {
                buffer[0] = '"';
                buffer[^1] = '"';

                result = ConvertCore(data, buffer, 1);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
        else
        {
            Span<char> buffer = stackalloc char[length];

            buffer[0] = '"';
            buffer[^1] = '"';

            result = ConvertCore(data, buffer, 1);
        }

        return result;
    }

    private static string ConvertCore(ReadOnlySpan<byte> data, Span<char> hexBuffer, int offset)
    {
        unchecked
        {
            int n = offset;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];

                byte b1 = (byte)(b >> 4);
                byte b2 = (byte)(b & 0xF);

                hexBuffer[n++] = (b1 < 10) ? (char)('0' + b1) : (char)('A' + (b1 - 10));
                hexBuffer[n++] = (b2 < 10) ? (char)('0' + b2) : (char)('A' + (b2 - 10));
            }
        }

        return new string(hexBuffer);
    }
}
